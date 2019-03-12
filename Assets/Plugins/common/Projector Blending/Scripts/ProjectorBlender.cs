/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2018 Daniel Jackson.
*
* This code can be used by members of the Wearable Computer Lab, University
* of South Australia for research purposes. Commercial use is not allowed
* without written permission. This copyright notice is subject to change.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySARCommon.Rendering;

// Put this into the Projector namespace in UnitySARCommon
namespace UnitySARCommon.Projector
{
    // Attach to a Camera with a MaskEffect. Will generate a bitmap which masks areas parts of the scene geometry so that they are not illuminated by multiple projectors simultaniously.
    [RequireComponent(typeof(MaskEffect))]
    [RequireComponent(typeof(Camera))]
    public class ProjectorBlender : MonoBehaviour
    {
        // Serialized members
        [System.Serializable]   // Enum for different start up behaviours.
        enum InitMode { None = 0, Generate = 1, File = 2 }; // None = do nothing on Start. Generate = on Start, analyse geometry and build mask (slow). File = on Start load mask from file specified by filePath.
        [SerializeField]
        private InitMode initMode;  // The behaviour to use when Start is called.
        [SerializeField]
        private string filePath; // Path to PNG file containing mask bitmap. If initMode = File, mask will be loaded from PNG during Start().
        [SerializeField]
        private Texture2D mask; // The mask applied to the rendered output.

        // Cached Component References
        private Camera cam; // Cached value, gameObject's Camera Component.
        private MaskEffect maskEffect; // Cached value, gameObject's MaskEffect Component.
        // Status variables
        private bool generatingMask = false; // True if the mask generation co-routine is currently running.
        private float progress = 0.0f; // Progress on the mask generation co-routine. 0.0-1.0.
        private int x, y; // Last pixel analysed in mask generation co-routine. All pixels are analyed once to construct mask.

        // Called on Component instantiation. Sets cam and maskEffect to reference appropriate Components.
        private void Awake()
        {
            cam = GetComponent<Camera>();
            maskEffect = GetComponent<MaskEffect>();
        }

        // Called when Component is enabled for the first time after Awake. Behaviour as defined by initMode.
        private void Start()
        {
            if (!Application.isEditor)
            {
                // If set to generate mask on initialization, start the mask generation co-routine.
                if (initMode == InitMode.Generate)
                {
                    StartGeneratingBlendMask();
                }
                // If set to load mask from file, attempt to do this now.
                else if (initMode == InitMode.File)
                {
                    var manager = FindObjectOfType<ProjectorBlendBoss>();
                    var path = filePath;
                    if (manager != null)
                    {
                        // If there is a ProjectorBlendBoss in the scene and filePath is not absolute, prepend the ProjectorBlendBoss base path.
                        path = System.IO.Path.Combine(manager.maskBasePath, path);
                    }
                    // If there is not Boss base path, and filePath is not absolute, prepend the application persistant data path.
                    path = System.IO.Path.Combine(Application.persistentDataPath, path);
                    // If the specified file exist, create a bitmap texture from it. Apply the resultant texture as a mask to the Camera render output.
                    if (System.IO.File.Exists(path))
                    {
                        var bytes = System.IO.File.ReadAllBytes(path);
                        var tex = new Texture2D(0, 0);
                        tex.LoadImage(bytes);
                        tex.filterMode = FilterMode.Point;
                        ApplyMask(tex);
                    }
                }
            }
        }

        // When this Component is removed, terminate any running co-routines. This prevents null reference issues.
        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        // Start generating a mask bitmap for this Projector. This will run in a co-routine, use GenerateProgress() to get the current progress.
        //  Calling this method while GenerateProgress != 1.0f will restart the generation algorithm, discarding existing intermediate data.
        // !! WARNING changing the scene geometry or the positions or intrinsic parameters of any cameras while mask generation is in progress WILL result in a incorrect blend mask.
        public void StartGeneratingBlendMask()
        {
            if (generatingMask)
            {
                Debug.LogWarning("StartGeneratingBlendMask called, but GenerateMask already in progress.", this);
            }
            // Ensure existing coroutines are stopped, then start the GenerateMask co-routine.
            StopAllCoroutines();
            StartCoroutine(GenerateMask());
        }

        // Apply a Texture2D as a mask to this Projector. Used internally, but can also be set to any image.
        public void ApplyMask(Texture2D mask)
        {
            this.mask = mask;
            maskEffect.Mask = mask;
        }

        // Returns the current mask completion as a float from 0.0 - 1.0.
        public float GenerateProgress()
        {
            if (!generatingMask)
            {
                return 1;
            }
            return progress;
        }

        /// <summary>
        /// Generic function prototype for pixel comparison function. Takes the area of a test pixel in world units, and an array of areas for overlapping pixels from other projectors and determines the mask opacity for the test pixel to achieve smooth blending. Each pixel in areaB is assumed to be from a different source projector and partially overlapping with the pixel with areaA.
        /// Different implementations of this function provide support for alternative blending algorithms.
        /// </summary>
        /// <param name="areaA">The pixel area of the pixel whos blending factor is to be determined.</param>
        /// <param name="areaB">Array of one pixel from each other projector in the blending group with the most overlap of the test pixel.</param>
        /// <returns>A float from 0 to 1 representing the mask weight for the test pixel.</returns>
        private delegate float AreaCompFunction(float areaA, float[] areaB);

        // Generates a mask for this projector. For each pixel of the image plane of the attached Camera Component, finds any overlapping pixels which are to be rendered by other Cameras with ProjectorBlender components, compares their areas and determines the mask bitmap which best blends these projectors, creating a projected SAR area without 'hotspots' where projectors overlap.
        private IEnumerator GenerateMask()
        {
            // Setup member vars to reflect progress.
            generatingMask = true;
            progress = 0;
            x = 0;
            y = 0;
            // This is it for the first frame, all actual processing will occur in the co-routine.
            yield return null;
            // Cache other Projectors in the scene. Except this one.
            var others = new List<ProjectorBlender>(FindObjectsOfType<ProjectorBlender>());
            others.Remove(this);
            // Get camera parameters and initialize the mask bitmap to match the image resolution.
            var width = cam.pixelWidth;
            var height = cam.pixelHeight;
            var tex = new Texture2D(cam.pixelWidth, cam.pixelHeight);
            // Set pixel values representing blend weights 0.0 and 1.0. Results are LERP'd between these values.
            var blend1 = Color.white;
            var blend0 = Color.black;
            // Set the area comparison function to perform a simple binary comparison where only the highest DPI pixel is rendered.
            AreaCompFunction compFunction = AreaCompBinary;
            // Setup timer, co-routine will execute for a fixed amount of time, then return control for the remainder of the frame.
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            // Iterate over pixels in image plane.
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    // Center of pixel on near clip plane in screen coords.
                    var screenPosNear = new Vector3(x + 0.5f, y + 0.5f, cam.nearClipPlane);
                    // Ray cast from camera origin through pixel center.
                    var ray = cam.ScreenPointToRay(screenPosNear);
                    var hitInfo = new RaycastHit();
                    if (Physics.Raycast(ray, out hitInfo))
                    {
                        // Get an infinte plane, co-planar with the incident facet.
                        var surface = new Plane(hitInfo.normal, hitInfo.point);
                        var candidatePixels = new List<float>();
                        // Calculate the area of this pixel in world units. This is the area which this pixel will illuminate in  the SAR environment.
                        var areaThis = ProjectedPixelArea(cam, new Vector3(x, y, cam.nearClipPlane), surface);
                        foreach (var o in others)
                        {
                            // Find pixels rendered by other Projectors which overlap this pixel.
                            //  This is done by reprojecting from the incident point of the original pixel back onto the image plane of the other projector.
                            var otherCam = o.GetComponent<Camera>();
                            var screenPos = otherCam.WorldToScreenPoint(hitInfo.point);
                            screenPos = new Vector3(Mathf.Floor(screenPos.x), Mathf.Floor(screenPos.y));
                            // Test that the reprojected pixel coords are within the cameras clip space, if they are not, ignore this pixel.
                            if (screenPos.x >= 0 && screenPos.x < otherCam.pixelWidth && screenPos.y >= 0 && screenPos.y < otherCam.pixelHeight && screenPos.z <= 1 && screenPos.z >= 0)
                            {
                                var oRay = otherCam.ScreenPointToRay(screenPos + new Vector3(0.5f, 0.5f, 0));
                                var oHitInfo = new RaycastHit();
                                // Raycast from the other cameras perspective, this determines if the other camera's view of the geometry is obstructed, in which case, this pixel should be ignored.
                                if (Physics.Raycast(oRay, out oHitInfo))
                                {
                                    if (oHitInfo.collider.GetInstanceID() == hitInfo.collider.GetInstanceID())
                                    {
                                        // If this pixel does indeed overlap in world space (covers some of the same area, on the same geometry) calculate it's area and add it to the queue to be analysed.
                                        var areaOther = ProjectedPixelArea(otherCam, screenPos, surface);
                                        candidatePixels.Add(areaOther);
                                    }
                                }
                            }
                        }
                        // Calculate blend weight.
                        var blendWeight = compFunction(areaThis, candidatePixels.ToArray());
                        // Update mask bitmap
                        tex.SetPixel(x, y, Color.Lerp(blend0, blend1, blendWeight));
                    }
                    else
                    {
                        // If the pixel hits no geometry, it may as well be masked.
                        tex.SetPixel(x, y, blend0);
                    }
                    // Check the time elapsed on this algorithm so far this Update. If it exceeds 100ms, yield.
                    if (stopwatch.ElapsedMilliseconds > 100)
                    {
                        // Update progress.
                        progress = (width * y + x) / ((float)width * height);
                        this.x = x;
                        this.y = y;
                        yield return null;
                        // Yield returns here, reset the timer.
                        stopwatch.Reset();
                        stopwatch.Start();
                    }
                }
            }
            // Upload the bitmap
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            // Set up the MaskEffect Component to use the new bitmap.
            ApplyMask(tex);
            // Set status vars to reflect completion.
            progress = 1.0f;
            this.x = 0;
            this.y = 0;
            generatingMask = false;
        }

        // Returns 1.0 if all areas are larger than or equal to area. Binary compare.
        private float AreaCompBinary(float area, float[] areas)
        {
            foreach (var compArea in areas)
            {
                if (area > compArea) return 0.0f;
            }
            return 1.0f;
        }

        // Returns the area in world units of a pixel from the source camera being projected onto the target plane.
        private float ProjectedPixelArea(Camera source, Vector3 pixel, Plane target)
        {
            // Create array of offsets representing the 4 corners of the pixel in screen units.
            var offsets = new Vector3[]
            {
            Vector3.zero,
            Vector3.up,
            Vector3.right + Vector3.up,
            Vector3.right,
            };
            var verts = new Vector3[offsets.Length];
            // Iterate over corner offsets, for each, cast a ray from the camera, through the given pixel + offset, to the target plane.
            //  The incident points of these raycasts are the quad vertices in world space.
            for (var i = 0; i < offsets.Length; i++)
            {
                var dist = 0.0f;
                var ray = source.ScreenPointToRay(pixel + offsets[i]);
                if (!target.Raycast(ray, out dist))
                {
                    Debug.LogError("Could not establish pixel dimensions.", this);
                    throw new System.Exception("Could not establish pixel dimensions.");
                }
                verts[i] = ray.origin + ray.direction.normalized * dist;
            }
            // Calculate the area of the pixel, projected onto the target plane.
            return QuadArea(verts[0], verts[1], verts[2], verts[3], target.normal);
        }

        // Return area of a planar, convex quad with vertexes a, b, c and d and with surface normal of normal.
        private float QuadArea(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal)
        {
            return Vector3.Dot(normal, Vector3.Cross(c - a, d - b)) * 0.5f;
        }
    }
}
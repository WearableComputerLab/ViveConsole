﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;

public class KeywordListener : MonoBehaviour
{
    GrammarRecognizer grammarRecognizer;
    KeywordRecognizer keywordRecognizer;
    public string[] keywords;
    public ConfidenceLevel confidence;
    public bool initialiseRecognizers;

    public class KeywordDetectedEvent : UnityEngine.Events.UnityEvent<string> { };

    public static KeywordDetectedEvent keywordHeard = new KeywordDetectedEvent();

    // Start is called before the first frame update
    void Start()
    {
        if (initialiseRecognizers)
            InitialiseRecognizers();
    }

    private void InitialiseRecognizers()
    {
        var grammarFilePath = GenerateGrammarFile();
        grammarRecognizer = new GrammarRecognizer(grammarFilePath, confidence);
        grammarRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        grammarRecognizer.Start();

        keywordRecognizer = new KeywordRecognizer(keywords, confidence);
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();
        PhraseRecognitionSystem.OnStatusChanged += 
            (_) =>
            {
                Debug.Log($"PRS status changed to {_}");
            };
    }

    public void StartListening()
    {
        if (!grammarRecognizer.IsRunning)
            grammarRecognizer.Start();

        if (!keywordRecognizer.IsRunning)
            keywordRecognizer.Start();
    }

    public void StopListening()
    {
        if (grammarRecognizer.IsRunning)
            grammarRecognizer.Stop();

        if (keywordRecognizer.IsRunning)
            keywordRecognizer.Stop();

        PhraseRecognitionSystem.Shutdown();
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log($"Keyword \"{args.text}\" detected\n[Confidence={args.confidence}]");
        if (args.semanticMeanings != null)
        {
            var semantics = "";
            foreach (var sm in args.semanticMeanings)
            {
                semantics += sm.key;
                foreach (var v in sm.values)
                    semantics += ":" + v;
                semantics += " || ";
            }
            Debug.Log(semantics);
            keywordHeard?.Invoke(args.semanticMeanings.Where(m => m.key == "Command").First().values[0]);
        }
        else
            keywordHeard?.Invoke(args.text);
    }

    private void OnDestroy()
    {
        grammarRecognizer?.Dispose();
        keywordRecognizer?.Dispose();
    }

    private string GenerateGrammarFile()
    {
        var path = System.IO.Path.Combine(Application.streamingAssetsPath, "myGrammar.xml");
        //var path = System.IO.Path.Combine(Application.streamingAssetsPath, "solitaireGrammar.xml");

        //        var rawText = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
        //<grammar version=""1.0"" xml:lang=""en-US"" mode=""voice"" root=""toplevel""
        //xmlns=""http://www.w3.org/2001/06/grammar"" tag-format=""semantics/1.0"">
        //  <rule id=""toplevel"" scope=""public"">
        //    <one-of>
        //      <item> <ruleref uri=""#PlayCard""/> <tag> out.Card = rules.PlayCard; </tag> </item>
        //      <item> <ruleref uri=""#MoveCard""/> <tag> out.MoveCard = rules.MoveCard; </tag> </item>
        //    </one-of>
        //  </rule>

        //  <rule id=""PlayCard"" scope=""public"">
        //    <example> please play the red queen </example>
        //    <example> play the ace </example>
        //    <example> play the five of diamonds please </example>

        //    <item repeat=""0-1""> please </item>
        //    <item>play the</item>
        //    <ruleref uri=""#Card""/>
        //    <tag> out = rules.Card; </tag>
        //    <item repeat=""0-1""> please </item>
        //  </rule>

        //  <rule id=""MoveCard"">
        //    <example>move the five of clubs to the six of hearts</example>
        //    <example>please put the jack of hearts on the ten of clubs</example>

        //    <item repeat=""0-1""> please </item>
        //    <one-of>
        //      <item> move the</item>
        //      <item> put the</item>
        //    </one-of>
        //    <ruleref uri=""#Card""/>
        //    <tag> out.FromCard = rules.Card; </tag>
        //    <item repeat=""0-1"">
        //      <one-of>
        //        <item> on the</item>
        //        <item> to the</item>
        //      </one-of>
        //      <ruleref uri=""#Card""/>
        //      <tag> out.ToCard = rules.latest(); </tag> 
        //    </item>
        //    <item repeat=""0-1""> please </item>
        //  </rule>

        //  <rule id=""Card"">
        //    <example> red queen </example>
        //    <example> jack of clubs </example>
        //    <example> ace </example>
        //    <example> spade </example>

        //    <one-of>
        //      <item>
        //        <!-- color and rank form -->
        //        <ruleref uri=""#Color""/>
        //        <tag> out.Color = rules.Color; </tag>
        //        <ruleref uri=""#Rank""/>
        //        <tag> out.Rank = rules.Rank; </tag>
        //      </item>

        //      <item>
        //        <!-- rank and optional suit form -->
        //        <ruleref uri=""#Rank""/>
        //        <tag> out.Rank = rules.Rank; </tag>
        //        <item repeat=""0-1"">
        //          of
        //          <ruleref uri=""#Suits""/>
        //          <tag> out.Suit = rules.Suits; </tag>
        //        </item>
        //      </item>

        //      <item>
        //        <!-- suit only form -->
        //        <ruleref uri=""#Suit""/>
        //        <tag> out.Suit = rules.Suit; </tag>
        //      </item>
        //    </one-of>
        //  </rule>

        //  <rule id=""Color"">
        //    <example> red </example>
        //    <example> black </example>

        //    <one-of>
        //      <item> red <tag> out = ""Red""; </tag> </item>
        //      <item> black <tag> out = ""Black""; </tag> </item>
        //     </one-of>
        //  </rule>

        //  <rule id=""Suit"">
        //    <example> spade </example>
        //    <example> club </example>

        //    <one-of>
        //      <item> club <tag> out = ""Club""; </tag> </item>
        //      <item> heart <tag> out = ""Heart""; </tag> </item>
        //      <item> diamond <tag> out = ""Diamond""; </tag> </item>
        //      <item> spade <tag> out = ""Spade""; </tag> </item>
        //     </one-of>
        //  </rule>

        //  <rule id=""Suits"">
        //    <example> spades </example>
        //    <example> clubs  </example>

        //    <one-of>
        //      <item> clubs  <tag> out = ""Club""; </tag> </item>
        //      <item> hearts <tag> out = ""Heart""; </tag> </item>
        //      <item> diamonds <tag> out = ""Diamond""; </tag> </item>
        //      <item> spades <tag> out = ""Spade""; </tag> </item>
        //     </one-of>
        //  </rule>

        //  <rule id=""Rank"">
        //    <example> ace </example> 
        //    <example> five </example> 
        //    <example> king </example> 
        //    <example> jack </example> 

        //    <one-of>
        //      <item> ace <tag> out = 1; </tag> </item>
        //      <item> two <tag> out = 2; </tag> </item>
        //      <item> three <tag> out = 3; </tag> </item>
        //      <item> four <tag> out = 4; </tag> </item>
        //      <item> five <tag> out = 5; </tag> </item>
        //      <item> six <tag> out = 6; </tag> </item>
        //      <item> seven <tag> out = 7; </tag> </item>
        //      <item> eight <tag> out = 8; </tag> </item>
        //      <item> nine <tag> out = 9; </tag> </item>
        //      <item> ten <tag> out = 10; </tag> </item>
        //      <item> jack <tag> out = 11; </tag> </item>
        //      <item> queen <tag> out = 12; </tag> </item>
        //      <item> king <tag> out = 13; </tag> </item>
        //      <item> lady <tag> out = 12; </tag> </item>
        //      <item> emperor <tag> out = 13; </tag> </item>
        //    </one-of>
        //  </rule>
        //</grammar>";
        //System.IO.File.WriteAllText(path, rawText);
        return path;
    }
}

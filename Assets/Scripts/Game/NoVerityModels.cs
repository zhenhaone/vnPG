using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoVerity.GameScene
{
    public enum SuspectId { Arthur, Charles, Beatrice }
    public enum EvidencePower { Weak, Strong, Calm }
    public enum EvidenceTrait { Motive, Method, Supplement, None }
    public enum TestimonyTruth { None, Useless, True, False, Unquestioned }

    [Serializable]
    public class EvidenceDefinition
    {
        public string id;
        public string title;
        [TextArea] public string description;
        public EvidencePower power;
        public EvidenceTrait trait;
        [TextArea] public string lowResponse;
        [TextArea] public string trueResponse;
        [TextArea] public string highResponse;
        [TextArea] public string calmResponse;

        public EvidenceDefinition(string id, string title, EvidencePower power,
            EvidenceTrait trait, string low, string truth, string high, string calm = "")
        {
            this.id = id; this.title = title; this.description = title;
            this.power = power; this.trait = trait;
            lowResponse = low; trueResponse = truth; highResponse = high; calmResponse = calm;
        }
    }

    [Serializable]
    public class SuspectDefinition
    {
        public SuspectId id;
        public string displayName;
        public string label;
        public int initialTension;
        public int pressureModifier;
        [TextArea] public string opening;
        public List<EvidenceDefinition> evidence = new List<EvidenceDefinition>();
    }

    [Serializable]
    public class TestimonyRecord
    {
        public SuspectId suspect;
        public string evidenceId;
        public string evidenceTitle;
        public EvidenceTrait trait;
        public TestimonyTruth truth;
        public int tension;
        public string response;
        public bool questioned;
    }

    [Serializable]
    public class RandomEventDefinition
    {
        public string id;
        public SuspectId? suspect;
        public string text;
        public int tensionChange;

        public RandomEventDefinition(string id, SuspectId? suspect, string text, int change)
        { this.id = id; this.suspect = suspect; this.text = text; tensionChange = change; }
    }
}

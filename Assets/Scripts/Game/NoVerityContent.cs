using System.Collections.Generic;

namespace NoVerity.GameScene
{
    public static class NoVerityContent
    {
        private static EvidenceDefinition E(string id, string title, EvidencePower power, EvidenceTrait trait, string low, string truth, string high, string calm = "")
            => new EvidenceDefinition(id, title, power, trait, low, truth, high, calm);

        public static List<SuspectDefinition> CreateSuspects() => new List<SuspectDefinition> { CreateArthur(), CreateCharles(), CreateBeatrice() };

        private static SuspectDefinition CreateArthur()
        {
            var s = new SuspectDefinition { id=SuspectId.Arthur, displayName="Arthur Bell", label="Arrogant / Controlling / High Resistance", initialTension=18, pressureModifier=-8,
                opening="(Leaning back) Tell me. Where do you want to begin?\nDetective: With your relationship with Clara.\nArthur: Relationship? We were only... ordinary friends." };
            s.evidence.Add(E("A_W1","Intimate Chat History",EvidencePower.Weak,EvidenceTrait.Supplement,"Chat logs? Is it strange for friends to discuss private matters?","...Yes, we were close in private. We never admitted the relationship.","It was one-sided. I never contacted her first."));
            s.evidence.Add(E("A_W2","Pregnancy Searches on Clara's Phone",EvidencePower.Weak,EvidenceTrait.Motive,"What she searched for has nothing to do with me.","...She told me she was pregnant. I told her to end it. She refused.","Pregnant? I do not know whose child it was. It was not mine."));
            s.evidence.Add(E("A_W3","Surveillance: Arthur Leaving Quickly",EvidencePower.Weak,EvidenceTrait.Supplement,"I had urgent business, so I left early.","She was bleeding when I left. I was afraid my life would be ruined.","The footage is forged. I was never there."));
            s.evidence.Add(E("A_W4","Orders to Isolate Clara",EvidencePower.Weak,EvidenceTrait.Supplement,"I only said she did not fit in. You call that isolation?","Yes, I told them to distance themselves from her. She was trouble.","That was Charles and Beatrice. I never said it."));
            s.evidence.Add(E("A_S1","Clara's Pregnancy Report",EvidencePower.Strong,EvidenceTrait.Motive,"...What does one report prove?","The child was mine. I demanded an abortion. She refused.","Only she knew who the father was. This proves nothing."));
            s.evidence.Add(E("A_S2","Blood and Impact Marks on the Table",EvidencePower.Strong,EvidenceTrait.Method,"...","We argued. I pushed her, and she struck the table. There was so much blood.","She fell on her own. I never pushed her."));
            s.evidence.Add(E("A_S3","Recording of the Abortion Threat",EvidencePower.Strong,EvidenceTrait.Method,"That proves nothing.","Yes, I said those things. But I did not kill her!","The recording was edited. You only heard the angry part."));
            s.evidence.Add(E("A_C1","An Old Group Photograph",EvidencePower.Calm,EvidenceTrait.None,"","","","(Looking at the photograph) We were young then. Nothing had happened yet."));
            s.evidence.Add(E("A_C2","Small Hotel Check-in Record",EvidencePower.Calm,EvidenceTrait.None,"","","","I went there after I ran. I could not return and destroy my future."));
            return s;
        }

        private static SuspectDefinition CreateCharles()
        {
            var s = new SuspectDefinition { id=SuspectId.Charles, displayName="Charles Reed", label="Elegant / Jealous / Balanced Resistance", initialTension=21, pressureModifier=0,
                opening="(Crossing one leg elegantly) What would you like to ask, detective?\nDetective: About Clara's death.\nCharles: Her death? I thought it was an accident." };
            s.evidence.Add(E("C_W1","Group Chat Mocking Clara",EvidencePower.Weak,EvidenceTrait.Supplement,"It was only a joke. Friends speak harshly sometimes.","Yes, I mocked her. She did not belong with us.","Arthur wrote those messages. I only agreed."));
            s.evidence.Add(E("C_W2","Order for Sedative Medication",EvidencePower.Weak,EvidenceTrait.Motive,"I suffer from insomnia. They were sleeping pills.","Yes, I bought sedatives. I only wanted her to be quiet.","That was not my order. My account was stolen."));
            s.evidence.Add(E("C_W3","Fingerprint on Clara's Cup",EvidencePower.Weak,EvidenceTrait.Method,"I poured her some water. That is normal.","Yes, I put the drug in her water and watched her drink.","I touched the cup. A fingerprint proves nothing."));
            s.evidence.Add(E("C_W4","Footprints Returning to the Cottage",EvidencePower.Weak,EvidenceTrait.Supplement,"I returned for a scarf I had forgotten.","I went back to check the drug. She was already lying in blood.","I never returned. Your analysis is wrong."));
            s.evidence.Add(E("C_S1","Drug Metabolism Report",EvidencePower.Strong,EvidenceTrait.Method,"Medication during pregnancy is not unusual.","I gave her the drug, but the dose was too small to kill!","Drugs in her body do not prove I gave them to her."));
            s.evidence.Add(E("C_S2","Deleted Purchase Record",EvidencePower.Strong,EvidenceTrait.Supplement,"I routinely clear my purchase history.","Yes, I deleted it. I thought I had killed her and panicked.","I deleted nothing. The data was fabricated."));
            s.evidence.Add(E("C_S3","Messages to Beatrice after the Incident",EvidencePower.Strong,EvidenceTrait.Supplement,"...","Yes, I asked Beatrice to handle the scene. But I did not kill Clara!","Beatrice forged those messages to implicate me."));
            s.evidence.Add(E("C_C1","Forensic Dose Analysis",EvidencePower.Calm,EvidenceTrait.None,"","","","The dose was not fatal. I drugged her, but I did not kill her."));
            s.evidence.Add(E("C_C2","Burial Site Trace Comparison",EvidencePower.Calm,EvidenceTrait.None,"","","","There is no trace of me at the burial site. Of course there is not."));
            return s;
        }

        private static SuspectDefinition CreateBeatrice()
        {
            var s = new SuspectDefinition { id=SuspectId.Beatrice, displayName="Beatrice Hall", label="Sensitive / Unstable / Low Resistance", initialTension=26, pressureModifier=8,
                opening="(Avoiding your eyes) A-ask whatever you want...\nDetective: Where were you that night?\nBeatrice: I... I was home. I was home all night." };
            s.evidence.Add(E("B_W1","Private Messages Confessing Affection",EvidencePower.Weak,EvidenceTrait.Motive,"I wrote that carelessly. It means nothing.","Yes... I loved Charles. I would do anything Charles asked.","I wrote it years ago. It means nothing now."));
            s.evidence.Add(E("B_W2","Late-night Vehicle Route",EvidencePower.Weak,EvidenceTrait.Supplement,"I drove out to buy food.","Charles said something terrible had happened. I drove to the cottage and put Clara in the car.","I did not drive that night. Someone borrowed my car!"));
            s.evidence.Add(E("B_W3","Forest Soil on the Cuff",EvidencePower.Weak,EvidenceTrait.Method,"It must be from a hiking trip.","Yes... it came from that night. I washed it again and again.","That is garden soil. The test is wrong!"));
            s.evidence.Add(E("B_W4","Searches for a Shovel and Waterproof Bags",EvidencePower.Weak,EvidenceTrait.Supplement,"I wanted to plant flowers.","Charles said something had to be handled. I knew what those items were for.","I never searched for them. It is all fake!"));
            s.evidence.Add(E("B_S1","Shoeprint at the Burial Pit",EvidencePower.Strong,EvidenceTrait.Method,"Everyone owns shoes like those.","I buried her! But I thought she was already dead!","Those are not my shoes. You cannot prove it!"));
            s.evidence.Add(E("B_S2","Fibres from Clara's Clothing",EvidencePower.Strong,EvidenceTrait.Method,"...","They stuck to me when I carried her. Her clothes were covered in blood.","She borrowed my coat before. The fibres are old."));
            s.evidence.Add(E("B_S3","Message: I Will Take the Blame",EvidencePower.Strong,EvidenceTrait.Motive,"That message concerned something else.","I wrote it! I moved and buried her. I did everything!","Charles forced me to send that message!"));
            s.evidence.Add(E("B_C1","Long-term Anxiety Treatment Record",EvidencePower.Calm,EvidenceTrait.None,"","","","The doctor said I have a dependent personality. My emotions have never been stable."));
            s.evidence.Add(E("B_C2","Chemical Contact Screening",EvidencePower.Calm,EvidenceTrait.None,"","","","There were no drugs on me. I did not poison her."));
            return s;
        }

        public static List<RandomEventDefinition> CreateEvents() => new List<RandomEventDefinition> {
            new RandomEventDefinition("GOOD_01",null,"Lightning crosses the window. The silence eases the tension.",-8),
            new RandomEventDefinition("GOOD_02",null,"You offer a glass of water. The suspect calms down.",-8),
            new RandomEventDefinition("GOOD_A",SuspectId.Arthur,"A childhood memory softens Arthur's expression for a moment.",-8),
            new RandomEventDefinition("GOOD_C",SuspectId.Charles,"Charles straightens a sleeve and regains composure.",-8),
            new RandomEventDefinition("GOOD_B",SuspectId.Beatrice,"Beatrice takes several deep breaths and stops trembling.",-8),
            new RandomEventDefinition("NEUT_01",null,"The clock ticks. Silence lasts for several seconds.",0),
            new RandomEventDefinition("NEUT_02",null,"Distant thunder draws every eye toward the window.",0),
            new RandomEventDefinition("NEUT_03",null,"You search the case file for the next piece of evidence.",0),
            new RandomEventDefinition("BAD_01",null,"Thunder makes the lamps flicker. The suspect grows tense.",5),
            new RandomEventDefinition("BAD_02",null,"You slam the table. The suspect recoils.",5),
            new RandomEventDefinition("BAD_A",SuspectId.Arthur,"At Clara's name, Arthur clenches a fist.",5),
            new RandomEventDefinition("BAD_C",SuspectId.Charles,"A pearl accessory snaps. Charles turns pale.",5),
            new RandomEventDefinition("BAD_B",SuspectId.Beatrice,"A chair scrapes sharply. Beatrice jumps.",5)
        };
    }
}

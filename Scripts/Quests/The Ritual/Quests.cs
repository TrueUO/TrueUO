using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Regions;
using System;
using System.Linq;

namespace Server.Engines.Quests.RitualQuest
{
    public class ScalesOfADreamSerpentQuest : BaseQuest
    {
        public override QuestChain ChainID => QuestChain.Ritual;
        public override Type NextQuest => typeof(TearsOfASoulbinderQuest);

        public override object Title => 1151122;  // Ritual: Scales of a Dream Serpent

        public override object Description => 1151123;
        /*Greetings, adventurer.  Our  queen has need of your services again, should you be willing to come to her aid.<br><br>	
		As you may know, her Majesty has spent the past thousand years diligently researching ways in which she could defeat the
		Defiler.  I have personally assisted her in this effort and we have discovered a ritual which will magnify the cleansing
		magic that she originally used against him, potentially breaking the barrier that protects the Defiler’s body from harm.
		<br><br>	The ritual is ancient, dangerous, and requires many obscure components that are not easy to obtain. Her 
		Majesty has been successful in acquiring all but five of them over the years but, now, time is running out and she has
		asked me to task you with their acquisition. I have been provided with a list of the five remaining components and would
		like you to obtain them.<br><br>	The first component that I require is a handful of scales from a Dream Serpent.<br><br>
		Dream Serpents are mischievous but benevolent creatures that live off of magical energy.  They are extremely rare and 
		hard to catch, preferring to live in the realm of dreams rather than the physical realm. However, there is one location
		within the kingdom that you may find one.<br><br>	Journey once again to the Southwestern Flight Tower.  On your way to
		the Athenaeum, you may have noticed a clearing surrounded by four hillocks. This location is an intersection of ley lines
		which run throughout Ter Mur.  Ley lines are energized, concentrated strands of magical energy and thus they are 
		irresistible to the creatures.  If my calculations are correct, you should find a serpent and be able to interact with the
		creature. Heed my warning, adventurer: you should not seek to harm the beast. Offensive tactics must be avoided at all 
		costs, as these magical beings help keep the very energy of Ter Mur harnessed and in balance. Speak with the creature and
		ask for its aid, but do not harm it. When you have acquired the scales, return to me and I shall reward you and guide you
		towards the second component.*/

        public override object Refuse => 1151124;
        /*You do not wish to assist us? Then we shall wait for someone who does not wish to sit idly by while our people suffer. 
		Be gone from my sight, coward.*/

        public override object Uncomplete => 1151125;  // Were you able to acquire the scales? Please do not dally, adventurer!

        public override object Complete => 1151126;
        /*You have the scales? Excellent! <br><br>	We are now one step closer to completing the list of components for the ritual,
		my friend. With your assistance, I believe we will be able to acquire all of them just in time.<br><br>	Now, on to the next
		component.*/

        public ScalesOfADreamSerpentQuest()
        {
            AddObjective(new ObtainObjective(typeof(DreamSerpentScale), "Dream Serpent Scales", 1, 0x1F13, 0, 1976)); // TODO: Get ID
            AddReward(new BaseReward(1151384)); // The gratitude of the Gargoyle Queen and the next quest in the chain.
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt(); // version
        }
    }

    public class TearsOfASoulbinderQuest : BaseQuest
    {
        public override QuestChain ChainID => QuestChain.Ritual;
        public override Type NextQuest => typeof(PristineCrystalLotusQuest);

        public override object Title => 1151131;  // Ritual: Tears of a Soulbinder

        public override object Description => 1151132;
        /*I fear I must now send you to convene with a creature that may not prove as willing to help as the ones you have encountered 
		previously, my friend. I require the crystallized tear of a Soulbinder, a malevolent creature that prefers to roam the desolate
		edges of Ter Mur where the Void has consumed much of the area.<br><br><br>	Spawned from the depths of the Abyss, the 
		Soulbinder is a vile and disgusting creature that prefers to consume its victims alive, devouring their souls during its
		digestion. It is the spirit of its prey that nourishes the beast, and this hunger is the only thing the beast knows. I 
		know not how you will cause one of these monsters to shed a tear, but I believe you are resourceful enough to find a way.
		<br><br><br>	Journey to the northeast, and follow the twisting strips of land surrounded by the Void; you should be able
		to locate a path to the lair of this foul thing. I wish I could provide you with guidance regarding approaching the beast,
		but I have been unable to discern anything beyond its location. Be on your guard, adventurer, and be prepared to do battle
		should the beast prove hostile.<br><br><br>*/

        public override object Refuse => 1151124;
        /*You do not wish to assist us? Then we shall wait for someone who does not wish to sit idly by while our people suffer. 
		Be gone from my sight, coward.*/

        public override object Uncomplete => 1151133;  // Were you able to obtain the Soulbinder's Tear?

        public override object Complete => 1151134;
        /*You continue to amaze me, my friend. I will admit I was concerned that the Soulbinder would prove to be too much of a 
		challenge and, yet, here you are. <br><br>Are you ready to go after the next component?*/

        public TearsOfASoulbinderQuest()
        {
            AddObjective(new ObtainObjective(typeof(SoulbinderTear), "Soulbinders Tears", 1, 0xE2A, 0, 2076));
            AddReward(new BaseReward(1151384)); // The gratitude of the Gargoyle Queen and the next quest in the chain.
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt(); // version
        }
    }

    public class PristineCrystalLotusQuest : BaseQuest
    {
        public override QuestChain ChainID => QuestChain.Ritual;
        public override Type NextQuest => null;

        public override object Title => 1151136;  // Ritual: Pristine Crystal Lotus

        public override object Description => 1151135;
        /*Now, you must traverse the northern desert. On the far edge, you will find a teleporter similar to that which you took
		to the Athenaeum Isle.  This will take you to a barren, small, and twisted piece of land that was long ago drained by the
		Void. <br><br>	The next component will be found there: a pristine crystal lotus. Long ago, what is now desert was a 
		beautiful field of these flowers. Sadly, the Defiler's magic drained the land of life and energy, causing the land to 
		become the wasteland it is today. However, one bloom remains, and it is that bloom you must acquire for me.<br><br>	In
		my research, I discovered that Queen Zhah’s predecessor, King Trajalem, had the only surviving bloom placed within a 
		protective barrier on the island I am directing you to. No one knows exactly why he did so, but I have journeyed there
		myself and seen it with my own eyes. Unfortunately, I was not able to discern how to break the barrier protecting the 
		lotus.<br><br>	The lotus stands on a pedestal surrounded by a configuration of magical tiles. I feel that these tiles
		are the key to breaking the barrier and obtaining the bloom.  However, their secret eluded me. What I was able to 
		discover through research is that you must speak the words ‘I seek the lotus’ to activate the tiles.  Additionally, if 
		you do manage to satisfy the secret of the tiles, you must state ‘Give me the lotus.’<br><br>	Journey to the area, my
		friend, and please obtain the lotus. If anyone can do it, I have faith that it is you.<br><br>	Be well, and I look
		forward to your return.<br><br>*/

        public override object Refuse => 1151124;
        /*You do not wish to assist us? Then we shall wait for someone who does not wish to sit idly by while our people suffer. 
		Be gone from my sight, coward.*/

        public override object Uncomplete => 1151137;  // Have you solved the secret of the lotus, my friend?

        public override object Complete => 1151138;
        /*Once again, you astound me with your perserverance and triumph! I cannot thank you enough. You are truly proving
		yourself a loyal friend.<br><br>Now, only two components remain.*/

        public int PuzzlesComplete { get; set; }
        public bool ReceivedLotus { get; set; }

        public PristineCrystalLotusQuest()
        {
            AddObjective(new ObtainObjective(typeof(PristineCrystalLotus), "Pristine Crystal Lotus", 1, 0x283B, 0, 1152));

            AddReward(new BaseReward(typeof(ChronicleOfTheGargoyleQueen2), 1151164)); // Chronicle of the Gargoyle Queen Vol. II
            AddReward(new BaseReward(typeof(TerMurSnowglobe), 1151172)); // Ter Mur Snowglobe
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(PuzzlesComplete);
            writer.Write(ReceivedLotus);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt(); // version

            PuzzlesComplete = reader.ReadInt();
            ReceivedLotus = reader.ReadBool();
        }
    }

    public class CatchMeIfYouCanQuest : BaseQuest
    {
        public override object Title => 1151144;  // Catch Me If You Can!

        public override object Description => 1151145;
        /*Oh, have you come to see me?<br><br>	This is wonderful! I haven’t spoken to a mortal in so long.  It can get pretty 
		droll here in the realm of dreams, and I have no one to play with! There aren’t many of us left. <br><br>	What’s that?
		You want something? <br><br>	My scales! Ha ha! You mortals are so silly, always after something or other. <br><br>	
		Well, I’ll happily give you some scales if you agree to play a game with me! I love games, don’t you? <br><br>	The game
		is simple:  if you can hit me 6 times, then you win!  Take this stone and step into the circle here. Then, use the stone
		and it will take you to my favorite place to play! <br><br>	So, shall we play? Excellent! Be sure to put away your pets
		before you teleport with me!<br>*/

        public override object Refuse => 1151146;  // You don’t want to play? Boo! Go away!

        public override object Uncomplete => 1151147;  // Ah ha! Looks like you are not very fast. Want to play again?

        public override object Complete => 1151148;
        /*Boo, I don’t like to lose! But wasn’t that fun? <br><br>	Here are some of my scales; give them to the Gargoyle Queen with 
		my blessing and tell her she needs to come play a game with me sometime!*/

        public DreamSerpentCharm Charm { get; set; }

        public CatchMeIfYouCanQuest()
        {
            AddObjective(new InternalObjective());
            AddReward(new BaseReward(typeof(DreamSerpentScale), 1151167)); // Dream Serpents Scale
        }

        public override void OnAccept()
        {
            base.OnAccept();

            Charm = new DreamSerpentCharm();
            Owner.AddToBackpack(Charm);
        }

        public override void RemoveQuest(bool removeChain)
        {
            base.RemoveQuest(removeChain);

            if (Charm != null && !Charm.Deleted)
            {
                Charm.Delete();
                Charm = null;
            }
        }

        public class InternalObjective : BaseObjective
        {
            public override object ObjectiveDescription => 1151213;  // Hit the Dream Serpent 6 times before the time is up.

            public InternalObjective()
                : base(6)
            {
            }

            public override bool Update(object o)
            {
                CurProgress++;

                if (Quest.Completed)
                {
                    // No Gump, no message, nothing.
                }

                return true;
            }

            public override void Serialize(GenericWriter writer)
            {
                base.Serialize(writer);
                writer.Write(0);
            }

            public override void Deserialize(GenericReader reader)
            {
                base.Deserialize(reader);
                reader.ReadInt(); // version
            }
        }

        public class BexilRegion : BaseRegion
        {
            public override bool AllowHousing(Mobile from, Point3D p)
            {
                return false;
            }

            public static void Initialize()
            {
                new BexilRegion();
            }

            public BexilRegion()
                : base("Bexil Region", Map.TerMur, DefaultPriority, new Rectangle2D(386, 3356, 35, 51))
            {
                Register();
                SetupRegion();
            }

            private void SetupRegion()
            {
                Map map = Map.TerMur;

                for (int x = 390; x < 408; x++)
                {
                    int z = map.GetAverageZ(x, 3360);

                    if (map.FindItem<Blocker>(new Point3D(x, 3360, z)) == null)
                    {
                        Blocker blocker = new Blocker();
                        blocker.MoveToWorld(new Point3D(x, 3360, z), map);
                    }
                }

                if (!GetEnumeratedMobiles().Any(m => m is BexilPunchingBag && !m.Deleted))
                {
                    BexilPunchingBag bex = new BexilPunchingBag();
                    bex.MoveToWorld(new Point3D(403, 3391, 38), Map.TerMur);
                }
            }

            public override bool CheckTravel(Mobile traveller, Point3D p, Spells.TravelCheckType type)
            {
                return false;
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);

            writer.Write(Charm);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt(); // version

            Charm = reader.ReadItem() as DreamSerpentCharm;
        }
    }

    public class FilthyLifeStealersQuest : BaseQuest
    {
        public override object Title => 1151155;  // Filthy Lifestealers!

        public override object Description => 1151154;
        /*Ah, what do we have here? <br><br><br>	Bah! I was so hoping you would be something other than a human, gargoyle,
		or elf. Do you know how many of them I have eaten? Your souls have filled me but, now, they are no longer appealing.
		You are in luck that I have recently eaten, however, or else I would have devoured you anyway. <br><br><br>	You want 
		something, yes? I can smell the desire for something on your soul.  Desire used to be such a lovely dessert for me but
		now it is bland and droll. <br><br><br>	My tears, eh? Ha! Such an odd request. But you have my interest piqued. 
		<br><br><br>	I’ll tell you what;  do something for me and I will give you a bottle of tears.  <br><br><br>	In 
		the northwest, walking the lava shores, is a creature that has vexed me for centuries. An old foe, the Lifestealer 
		eats the hearts of its victims, consuming their essence in a similar fashion to how I consume their souls.   Many times
		have I lost a victim to these inane creatures, and many times have I attempted to exact my vengeance.  Unfortunately, 
		due to my corpulent nature, I am not as fast as they are and have failed in my desire to kill them.  Wouldn’t it be 
		poetic? A Soulbinder consuming a Lifestealer! <br><br><br>	Go to the lava shores in the northwest, mortal, and kill 
		the lifestealers you find there.  When you have thinned them out, return to me and I will give you what you seek.
		<br><br><br>*/

        public override object Refuse => 1151156;
        /*You dare refuse me? You are truly an imbecile. Ah well, I guess I shall eat you, after all! Just as soon as I finish 
		digesting my last meal, that is.*/

        public override object Uncomplete => 1151157;  // Have you killed the Lifestealers yet? Don’t try my patience, or I will eat you!

        public override object Complete => 1151158;
        /*I can smell their deaths upon you, mortal. Each Lifestealer you killed let loose all of the souls that they had taken, 
		and I was able to draw them here with my magic. I am so full, now! I think this will hold me over for quite some time. 
		<br><br><br>	Here, take what you came for. While you were gone, the thought of those Lifestealers dying was enough to
		make me laugh so hard I cried. I filled this bottle for you. <br><br>	Off with you, now! <br>*/

        public FilthyLifeStealersQuest()
        {
            AddObjective(new SlayObjective(typeof(Lifestealer), "Life Stealers", 10));

            AddReward(new BaseReward(typeof(SoulbinderTear), 1151170)); // Souldbinder's Tears
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt(); // version
        }
    }

    public class HairOfTheDryadQueen : BaseQuest
    {
        public override QuestChain ChainID => QuestChain.Ritual2;
        public override Type NextQuest => typeof(HeartOfTheNightTerror);

        public override object Title => 1151127; // Ritual: Hair of the Dryad Queen

        public override object Description => 1151128;
        /*Hail, adventurer.<br><br> I have heard of your continued assistance and loyalty to our Queen, and I cannot thank you enough for what you have done.  
         * Our darkest hour is approaching, and you have truly proven a valuable ally. <br><br>The Queen and her Advisor have begun preparing the empowerment ritual with the previous components you gathered. 
         * They have made excellent progress and have informed me they are almost ready for the final two components.  Thus, I have been tasked with pointing you in the right direction.<br><br>	
         * First, they require a strand of hair from a dryad queen.<br><br>	Forces of nature unto themselves, dryad queens oversee the rule of the lush and verdant forests throughout our world.  
         * You must seek out Oakwhisper, Queen of the Fire Island dryad clan. She has aided us in the past and we hope she shall be willing do so once again.<br><br>	
         * Journey to Fire Island and venture deep into the labyrinthine woods.  What you seek is an ancient, majestic tree that towers above all of the rest; it stands within a clearing full of life. <br><br> 
         * Hanging upon this tree is a magical standard; place your hand upon it and you will be transported to Queen Oakwhisper's bower.<br><br>Approach the Dryad Queen and ask for her assistance.*/

        public override object Refuse => 1151124;
        /*You do not wish to assist us? Then we shall wait for someone who does not wish to sit idly by while our people suffer. 
		Be gone from my sight, coward.*/

        public override object Uncomplete => 1151129; // Have you met with Queen Oakwhisper?

        public override object Complete => 1151130;
        /*You have acquired a strand of Oakwhisper's hair? You have my gratitude for your continued support. Now, for the next component.*/

        public HairOfTheDryadQueen()
        {
            AddObjective(new ObtainObjective(typeof(HairOfADryadQueen), "Hair of a Dryad Queen", 1, 0xC60, 0, 2563));
            AddReward(new BaseReward(1151384)); // The gratitude of the Gargoyle Queen and the next quest in the chain.
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt(); // version
        }
    }

    public class HeartOfTheNightTerror : BaseQuest
    {
        public override QuestChain ChainID => QuestChain.Ritual2;
        public override Type NextQuest => null;

        public override object Title => 1151139; // Ritual: Heart of the Night Terror

        public override object Description => 1151140;
        /*The bearer of the last component will be easy to find but extremely challenging to overcome, I fear. <br><br>	
         * In eastern Ter Mur there lies a ghost town. Formerly a fishing village, this town was long since abandoned due to the instability of the land it sits upon. 
         * Now, it is the haunting ground of a group of vile and insidious creatures:  the Night Terrors.<br><br>	
         * The Night Terrors walk the empty streets of the village, slaying any who venture into their path.  
         * It is the heart of one of these creatures that I require.  Journey to the village and slay one of the beasts. 
         * Once you have done so, cut it open and tear out its heart. That is the final ingredient that needed for the ritual.<br><br>
         * Be careful, as the beasts are fiercely protective of one another and are extremely powerful. I look forward to your return.<br><br>
         */

        public override object Refuse => 1151141;
        /*You have come this far and turn away now? I am disappointed in you, adventurer, as are my people. Should you regain your sense of honor, do let me know.*/

        public override object Uncomplete => 1151142; // Have you slain the beast and obtained its heart? Time is of the essence, friend; do not dally!

        public override object Complete => 1151143;
        /*The Night Terror is no more? I see by the heart in your hand that you have felled the beast. With this final component, you have given us more than a fighting chance against the Defiler.  
         * You have proven yourself a great and powerful ally to our people, and the Queen has asked that I provide you with this small token of appreciation.<br><br>
         * The time for battle is nigh, my friend, and I hope that you will stand by Queen Zhah when she goes to release the Defiler. With you standing by her, I know we shall succeed!
         */

        public HeartOfTheNightTerror()
        {
            AddObjective(new ObtainObjective(typeof(NightTerrorHeart), "Night Terror Heart", 1, 0x1CF0, 0, 96));
            AddReward(new BaseReward(typeof(ChronicleOfTheGargoyleQueen3), 1151165)); // Chronicle of the Gargoyle Queen Vol. III
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt(); // version
        }
    }

    public class KnowledgeOfNature : BaseQuest
    {
        public KnowledgeOfNature()
        {
            AddObjective(new QuestionAndAnswerObjective(4, m_EntryTable));
        }

        public override bool DoneOnce => true;
        public override bool ShowDescription => false;
        public override bool IsQuestionQuest => true;

        public override object Title => 1151149; // Knowledge of Nature

        /*Greetings, mortal. <br><br>	I sensed the aura of the Gargoyle Queen surrounding you, which is why you have been allowed entry into my bower.  
        * Zhah proved herself a friend to my people long ago when she aided us in escaping her crumbling lands .   <br><br>Do not think that will be enough for me to provide you with what you seek.  <br><br>	
        * I know that you require a strand of my hair, mortal; I am one with nature and can sense the thoughts of those who tread my forest.  While I understand you bear the intention of aiding the Gargoyle Queen, 
        * you must first prove yourself a friend of the forest before I will part with it. <br><br>	I ask that you answer a few questions to show that you are knowledgeable about nature.  A series of questions; no more, no less.  
        * If you answer them correctly and truthfully, I will bequeath a strand of my hair unto you. <br><br>	Do you accept?
        */
        public override object Description => 1151150;

        //No? You will not gain my help without proving yourself.  If you will not do so, then apparently the Gargoyle Queen was a fool to choose you.
        public override object Refuse => 1151151;

        //Focusing yourself, you prepare to show your knowledge about the natural world.
        public override object Uncomplete => 1151417;

        /*Indeed, you possess great knowledge of nature, mortal. You have surprised me. <br><br>	
         * Here, accept this strand of my hair with my blessing.  Tell the Gargoyle Queen that I wish her luck, 
         * and hope that she will see her land whole again. <br><br>	May the sun shine on you, mortal. Farewell.
         */
        public override object Complete => 1151153;

        /*
         * It would appear that you need to improve your knowledge of nature, mortal. 
         */
        public override object FailedMsg => 1151152;

        public override void OnAccept()
        {
            base.OnAccept();
            Owner.SendGump(new QAndAGump(Owner, this));
        }

        public override void GiveRewards()
        {
            base.GiveRewards();

            Owner.AddToBackpack(new HairOfADryadQueen());
            Owner.SendLocalizedMessage(1151414); // Hair of a Dryad Queen has been placed in your backpack.
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }

        public static void Configure()
        {
            m_EntryTable[0] = new QuestionAndAnswerEntry(1151329, new object[] { 1151330 }, new object[] { 1151331, 1151332 }); //At what age does an oak tree begin to produce acorns?
            m_EntryTable[1] = new QuestionAndAnswerEntry(1151333, new object[] { 1151337 }, new object[] { 1151334, 1151335, 1151336 }); //What is the process called in which a tree evaporates water through its leaves?
            m_EntryTable[2] = new QuestionAndAnswerEntry(1151338, new object[] { 1151341 }, new object[] { 1151339, 1151340, 1151342 }); //What is the average lifespan of an oak tree?
            m_EntryTable[3] = new QuestionAndAnswerEntry(1151343, new object[] { 1151347 }, new object[] { 1151344, 1151345, 1151346 }); //Of which family of trees is an oak?
            m_EntryTable[4] = new QuestionAndAnswerEntry(1151348, new object[] { 1151349 }, new object[] { 1151350, 1151351, 1151352 }); //What is unique about pine trees?
            m_EntryTable[5] = new QuestionAndAnswerEntry(1151353, new object[] { 1151356 }, new object[] { 1151354, 1151357, 1151358 }); //What is a pomaceous fruit?
            m_EntryTable[6] = new QuestionAndAnswerEntry(1151359, new object[] { 1151362 }, new object[] { 1151360, 1151361, 1151363 }); //This plant is known as ‘deadly nightshade:’
            m_EntryTable[7] = new QuestionAndAnswerEntry(1151364, new object[] { 1151360 }, new object[] { 1151361, 1151363, 1151365 }); //Which of the following plants possesses a fruit that is extremely poisonous and shaped like a spiked ball?
            m_EntryTable[8] = new QuestionAndAnswerEntry(1151366, new object[] { 1151369 }, new object[] { 1151367, 1151368, 1151370 }); //Which is the species of flower that produces vanilla?
            m_EntryTable[9] = new QuestionAndAnswerEntry(1151371, new object[] { 1151372 }, new object[] { 1151373 }); //A notch in the trunk of a tree will stay the same distance from the ground as the tree grows in height; true or false?
            m_EntryTable[10] = new QuestionAndAnswerEntry(1151374, new object[] { 1151375 }, new object[] { 1151376, 1151377, 1151378 }); //Which tree type is called the ‘axe breaker?’
            m_EntryTable[11] = new QuestionAndAnswerEntry(1151379, new object[] { 1151382 }, new object[] { 1151380, 1151381, 1151383 }); //Which commonly consumed vegetable can also to be used medicinally to treat burns, bee stings, and infections?
        }

        private static readonly QuestionAndAnswerEntry[] m_EntryTable = new QuestionAndAnswerEntry[12];
        public static QuestionAndAnswerEntry[] EntryTable => m_EntryTable;
    }
}

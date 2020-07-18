using System;
using Server.Mobiles;

namespace Server.Customs.Invasion_System
{
	public class MonsterTownSpawnEntry
	{
		#region MonsterSpawnEntries
		
		public static MonsterTownSpawnEntry[] Undead = new MonsterTownSpawnEntry[]
		{
			//Monster													//Percent
			new MonsterTownSpawnEntry( typeof( Zombie ),						80 ),
			new MonsterTownSpawnEntry( typeof( Skeleton ),						75 ),
			new MonsterTownSpawnEntry( typeof( SkeletalMage ),					70 ),
			new MonsterTownSpawnEntry( typeof( BoneKnight ),					60 ),
			new MonsterTownSpawnEntry( typeof( SkeletalKnight ),				55 ),
			new MonsterTownSpawnEntry( typeof( Lich ),							45 ),
			new MonsterTownSpawnEntry( typeof( Ghoul ),							40 ),
			new MonsterTownSpawnEntry( typeof( BoneMagi ),						35 ),
			new MonsterTownSpawnEntry( typeof( Wraith ),						30 ),
			new MonsterTownSpawnEntry( typeof( RottingCorpse ),					20 ),
			new MonsterTownSpawnEntry( typeof( LichLord ),						15 ),
			new MonsterTownSpawnEntry( typeof( Spectre ),						10 ),
			new MonsterTownSpawnEntry( typeof( Shade ),							5 ),
			new MonsterTownSpawnEntry( typeof( AncientLich ),					0 )
		};

		public static MonsterTownSpawnEntry[] Humanoid = new MonsterTownSpawnEntry[]
		{
			//Monster														//Percent
			new MonsterTownSpawnEntry( typeof( Brigand ),						80 ),
			new MonsterTownSpawnEntry( typeof( Executioner ),					75 ),
			new MonsterTownSpawnEntry( typeof( EvilMage ),						55 ),
			new MonsterTownSpawnEntry( typeof( EvilMageLord ),					45 ),
			new MonsterTownSpawnEntry( typeof( Ettin ),							40 ),
			new MonsterTownSpawnEntry( typeof( Ogre ),							30 ),
			new MonsterTownSpawnEntry( typeof( OgreLord ),						25 ),
			new MonsterTownSpawnEntry( typeof( ArcticOgreLord ),				20 ),
			new MonsterTownSpawnEntry( typeof( Troll ),							15 ),
			new MonsterTownSpawnEntry( typeof( Cyclops ),						5 ),
			new MonsterTownSpawnEntry( typeof( Titan ),							0 )
		};

		public static MonsterTownSpawnEntry[] OrcsandRatmen = new MonsterTownSpawnEntry[]
		{
			//Monster														//Percent
			new MonsterTownSpawnEntry( typeof( Orc ),							80 ),
			new MonsterTownSpawnEntry( typeof( OrcishMage ),					70 ),
			new MonsterTownSpawnEntry( typeof( OrcishLord ),					65 ),
			new MonsterTownSpawnEntry( typeof( OrcCaptain ),					55 ),
			new MonsterTownSpawnEntry( typeof( OrcBomber ),						45 ),
			new MonsterTownSpawnEntry( typeof( OrcBrute ),						40 ),
			new MonsterTownSpawnEntry( typeof( Ratman ),						20 ),
			new MonsterTownSpawnEntry( typeof( RatmanArcher ),					10 ),
			new MonsterTownSpawnEntry( typeof( RatmanMage ),					0 )
		};

		public static MonsterTownSpawnEntry[] Elementals = new MonsterTownSpawnEntry[]
		{
			//Monster														//Percent
			new MonsterTownSpawnEntry( typeof( EarthElemental ),				80 ),
			new MonsterTownSpawnEntry( typeof( AirElemental ),					70 ),
			new MonsterTownSpawnEntry( typeof( FireElemental ),					65 ),
			new MonsterTownSpawnEntry( typeof( WaterElemental ),				55 ),
			new MonsterTownSpawnEntry( typeof( SnowElemental ),					45 ),
			new MonsterTownSpawnEntry( typeof( IceElemental ),					40 ),
			new MonsterTownSpawnEntry( typeof( Efreet ),						20 ),
			new MonsterTownSpawnEntry( typeof( PoisonElemental ),				10 ),
			new MonsterTownSpawnEntry( typeof( BloodElemental ),				0 )
		};

		public static MonsterTownSpawnEntry[] OreElementals = new MonsterTownSpawnEntry[]
		{
			//Monster														//Percent
			new MonsterTownSpawnEntry( typeof( DullCopperElemental ),			80 ),
			new MonsterTownSpawnEntry( typeof( CopperElemental ),				60 ),
			new MonsterTownSpawnEntry( typeof( BronzeElemental ),				45 ),
			new MonsterTownSpawnEntry( typeof( ShadowIronElemental ),			35 ),
			new MonsterTownSpawnEntry( typeof( GoldenElemental ),				25 ),
			new MonsterTownSpawnEntry( typeof( AgapiteElemental ),				15 ),
			new MonsterTownSpawnEntry( typeof( VeriteElemental ),				5 ),
			new MonsterTownSpawnEntry( typeof( ValoriteElemental ),				0 )
		};

		public static MonsterTownSpawnEntry[] Ophidian = new MonsterTownSpawnEntry[]
		{
			//Monster														//Percent
			new MonsterTownSpawnEntry( typeof( OphidianWarrior ),				60 ),
			new MonsterTownSpawnEntry( typeof( OphidianMage ),					40 ),
			new MonsterTownSpawnEntry( typeof( OphidianArchmage ),				20 ),
			new MonsterTownSpawnEntry( typeof( OphidianKnight ),				10 ),
			new MonsterTownSpawnEntry( typeof( OphidianMatriarch ),				0 )
		};

		public static MonsterTownSpawnEntry[] Arachnid = new MonsterTownSpawnEntry[]
		{
			//Monster														//Percent
			new MonsterTownSpawnEntry( typeof( Scorpion ),						80 ),
			new MonsterTownSpawnEntry( typeof( GiantSpider ),					60 ),
			new MonsterTownSpawnEntry( typeof( TerathanDrone ),					40 ),
			new MonsterTownSpawnEntry( typeof( TerathanWarrior ),				30 ),
			new MonsterTownSpawnEntry( typeof( TerathanMatriarch ),				20 ),
			new MonsterTownSpawnEntry( typeof( TerathanAvenger ),				15 ),
			new MonsterTownSpawnEntry( typeof( DreadSpider ),					5 ),
			new MonsterTownSpawnEntry( typeof( FrostSpider ),					0 )
		};

		public static MonsterTownSpawnEntry[] Snakes = new MonsterTownSpawnEntry[]
		{
			//Monster														//Percent
			new MonsterTownSpawnEntry( typeof( Snake ),							80 ),
			new MonsterTownSpawnEntry( typeof( GiantSerpent ),					60 ),
			new MonsterTownSpawnEntry( typeof( LavaSnake ),						50 ),
			new MonsterTownSpawnEntry( typeof( LavaSerpent ),					40 ),
			new MonsterTownSpawnEntry( typeof( IceSnake ),						20 ),
			new MonsterTownSpawnEntry( typeof( IceSerpent ),					10 ),
			new MonsterTownSpawnEntry( typeof( SilverSerpent ),					0 )
		};

		public static MonsterTownSpawnEntry[] Abyss = new MonsterTownSpawnEntry[]
		{
			//Monster														//Percent
			new MonsterTownSpawnEntry( typeof( Gargoyle ),						75 ),
			new MonsterTownSpawnEntry( typeof( StoneGargoyle ),					50 ),
			new MonsterTownSpawnEntry( typeof( FireGargoyle ),					35 ),
			new MonsterTownSpawnEntry( typeof( Daemon ),						15 ),
			new MonsterTownSpawnEntry( typeof( IceFiend ),						5 ),
			new MonsterTownSpawnEntry( typeof( Balron ),						0 )
		};

		public static MonsterTownSpawnEntry[] DragonKind = new MonsterTownSpawnEntry[]
		{
			//Monster														//Percent
			new MonsterTownSpawnEntry( typeof( Wyvern ),						70 ),
			new MonsterTownSpawnEntry( typeof( Drake ),							45 ),
			new MonsterTownSpawnEntry( typeof( Dragon ),						25 ),
			new MonsterTownSpawnEntry( typeof( WhiteWyrm ),						15 ),
			new MonsterTownSpawnEntry( typeof( ShadowWyrm ),					5 ),
			new MonsterTownSpawnEntry( typeof( AncientWyrm ),					0 )
		};

		#endregion

		private Type m_Monster;
		private int m_Percent;

		public Type Monster { get { return m_Monster; } set { m_Monster = value; } }
		public int Percent { get { return m_Percent; } set { m_Percent = value; } }

		public MonsterTownSpawnEntry( Type monster, int percent )
		{
			m_Monster = monster;
			m_Percent = percent;
		}
	}
}
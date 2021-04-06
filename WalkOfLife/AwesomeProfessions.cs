﻿using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;

namespace TheLion.AwesomeProfessions
{
	/// <summary>The mod entry point.</summary>
	public partial class AwesomeProfessions : Mod
	{
		public static IContentHelper Content { get; set; }
		public static IModEvents Events { get; set; }
		public static IModRegistry ModRegistry { get; set; }
		public static IReflectionHelper Reflection { get; set; }
		public static ITranslationHelper I18n { get; set; }
		public static ModDataDictionary Data { get; set; }
		public static ProfessionsConfig Config { get; set; }
		public static EventManager EventManager { get; set; }
		public static ProspectorHunt ProspectorHunt { get; set; }
		public static ScavengerHunt ScavengerHunt { get; set; }
		public static string UniqueID { get; private set; }

		internal static int demolitionistBuffMagnitude;
		internal static uint bruteKillStreak;
		internal static readonly List<Vector2> initialLadderTiles = new();

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			// get unique id and generate buff ids
			UniqueID = ModManifest.UniqueID;
			int uniqueHash = (int)(Math.Abs(UniqueID.GetHashCode()) / Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(UniqueID.GetHashCode()))) - 8 + 1));
			Utility.SetProfessionBuffIDs(uniqueHash);

			// get mod helpers
			Content = helper.Content;
			Events = helper.Events;
			ModRegistry = helper.ModRegistry;
			Reflection = helper.Reflection;
			I18n = helper.Translation;

			// get configs.json
			Config = helper.ReadConfig<ProfessionsConfig>();

			// patch profession icons
			if (Config.UseModdedProfessionIcons) helper.Content.AssetEditors.Add(new IconEditor());

			// apply patches
			BasePatch.Init(Monitor);
			new HarmonyPatcher().ApplyAll(
				new AnimalHouseAddNewHatchedAnimalPatch(),
				new BasicProjectileBehaviorOnCollisionWithMonsterPatch(),
				new BasicProjectileCtorPatch(),
				new BobberBarCtorPatch(),
				new BushShakePatch(),
				new CaskPerformObjectDropInActionPatch(),
				new CrabPotCheckForActionPatch(),
				new CrabPotDayUpdatePatch(),
				new CrabPotDrawPatch(),
				new CraftingRecipeCtorPatch(),
				new CropHarvestPatch(),
				new FarmAnimalDayUpdatePatch(),
				new FarmAnimalGetSellPricePatch(),
				new FarmAnimalPetPatch(),
				new FarmerHasOrWillReceiveMailPatch(),
				new FarmerShowItemIntakePatch(),
				new FishingRodStartMinigameEndFunctionPatch(),
				new FishPondUpdateMaximumOccupancyPatch(),
				new FruitTreeDayUpdatePatch(),
				new Game1CreateObjectDebrisPatch(),
				new Game1DrawHUDPatch(),
				new Game1CreateItemDebrisPatch(),
				new Game1CreateObjectDebrisPatch(),
				new GameLocationBreakStonePatch(),
				new GameLocationCheckActionPatch(),
				new GameLocationDamageMonsterPatch(),
				new GameLocationGetFishPatch(),
				new GameLocationExplodePatch(),
				new GameLocationOnStoneDestroyedPatch(),
				new GeodeMenuUpdatePatch(),
				new GreenSlimeUpdatePatch(),
				new GreenSlimeGetExtraDropItemsPatch(),
				new HoeDirtApplySpeedIncreasesPatch(),
				new LevelUpMenuAddProfessionDescriptionsPatch(),
				new LevelUpMenuGetImmediateProfessionPerkPatch(),
				new LevelUpMenuGetProfessionNamePatch(),
				new LevelUpMenuGetProfessionTitleFromNumberPatch(),
				new LevelUpMenuRemoveImmediateProfessionPerkPatch(),
				new LevelUpMenuRevalidateHealthPatch(),
				new MeleeWeaponDoAnimateSpecialMovePatch(),
				new MineShaftCheckStoneForItemsPatch(),
				new ObjectCheckForActionPatch(),
				new ObjectCtorPatch(),
				new ObjectGetMinutesForCrystalariumPatch(),
				new ObjectGetPriceAfterMultipliersPatch(),
				new ObjectPerformObjectDropInActionPatch(),
				new PondQueryMenuDrawPatch(),
				new ProjectileBehaviorOnCollisionPatch(),
				new QuestionEventSetUpPatch(),
				new SlingshotPerformFirePatch(),
				new TemporaryAnimatedSpriteCtorPatch(),
				new TreeDayUpdatePatch(),
				new TreeUpdateTapperProductPatch()
			);

			// start event manager
			EventManager = new EventManager(Monitor);

			// add debug commands
			Helper.ConsoleCommands.Add("wol_addprofessions", "Get specified professions." + _GetCommandUsage(), _AddProfessionsToLocalPlayer);
			Helper.ConsoleCommands.Add("wol_checksubscribed", "List currently subscribed mod events.", _PrintSubscribedEvents);
			Helper.ConsoleCommands.Add("wol_checkdatafield", "Check current values for data fields." + _GetAvailableDataFields(), _PrintDataField);
		}
	}
}
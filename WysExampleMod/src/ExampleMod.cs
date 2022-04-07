using System.Diagnostics.CodeAnalysis;

using GmmlHooker;

using GmmlPatcher;

using UndertaleModLib;
using UndertaleModLib.Models;

using WysApi.Api;

using WysModMenu;

namespace WysExampleMod;

// ReSharper disable once UnusedType.Global
public class ExampleMod : IGameMakerMod {
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    private class Config {
        public int epilepsyWait { get; private set; } = 10;
    }

    public void Load(int audioGroup, ModData currentMod) {
        if(audioGroup != 0) return;
        Config config = GmmlConfig.Config.LoadPatcherConfig<Config>(audioGroup, "wysExampleMod.json");
        UndertaleData data = Patcher.data;

        // change epilepsy screen title to "eat my nuts"
        Hooker.HookCode("gml_Object_obj_epilepsy_warning_Create_0",
            @"#orig#()
txt_1 = ""eat my nuts""");

        // change epilepsy screen text to whatever is in the config file or "test" if it doesn't exist
        Hooker.HookCode("gml_Object_obj_epilepsy_warning_Create_0",
            @"#orig#()
var default_config = json_parse(""{ \""text\"": \""test\"" }"")
var config = gmml_config_load(""sample_mod_epilepsy_text.json"", default_config)
txt_2 = config.text");

        // restart the room and log "press" when you press F2
        Hooker.HookCode("gml_Object_obj_player_Step_0",
            @"#orig#()
if keyboard_check_pressed(vk_f2)
{
    room_restart()
    show_debug_message(""press"")
}
");

        // poor epilepsy warning, i'm hooking it for the third time already
        // change the default 180 delay until being able to skip the epilepsy screen to the configured value
        Hooker.HookAsm("gml_Object_obj_epilepsy_warning_Create_0", (code, locals) => {
            AsmCursor cursor = new(code, locals);
            cursor.GotoNext("pushi.e 180");
            cursor.Replace($"pushi.e {config.epilepsyWait}");
        });

        // log "move like a snail" every time scr_move_like_a_snail is executed
        // (commented out cuz it spams the log... a lot)
//        Hooker.HookFunction("scr_move_like_a_snail",
//            @"#orig#(argument0, argument1, argument2, argument3)
//show_debug_message(""move like a snail"")
//");

        // create the Test menu
        data.Variables.EnsureDefined("wys_test3", UndertaleInstruction.InstanceType.Global, false, data.Strings, data);
        data.Variables.EnsureDefined("wys_test4", UndertaleInstruction.InstanceType.Global, false, data.Strings, data);

        Hooker.CreateGlobalScript("scr_setup_menu_example", "global.wys_test3 = false\nglobal.wys_test4 = 0.0",
            0, out _);

        UndertaleGameObject configMenu = Menus.CreateMenu("ExampleConfig",
            new Menus.WysMenuOption("\"Test Option 1\"") {
                instance = Menus.Vanilla.Settings,
                tooltipScript = Menus.Vanilla.Tooltips.Text,
                tooltipArgument = "\"test option 1 tooltip\""
            },
            new Menus.WysMenuOption("\"Test Option 2\"") {
                instance = Menus.Vanilla.Settings,
                tooltipScript = Menus.Vanilla.Tooltips.Text,
                tooltipArgument = "\"test option 2 tooltip\""
            },
            Menus.CreateToggleOption("\"Test Option 3\"", "test3",
                "global.wys_test3 = argument0", "selectedItem = global.wys_test3", "global.wys_test3",
                Menus.Vanilla.Tooltips.Text, "\"test option 3 tooltip\""),
            Menus.CreateChangeOption("\"Test Option 4\"", "test4",
                "global.wys_test4 += argument0", "return string(global.wys_test4)", 0.1d),
            new Menus.WysMenuOption("\"Test Option 5\"") {
                script = "scr_end_game",
                tooltipScript = Menus.Vanilla.Tooltips.Text,
                tooltipArgument = "\"exit game\""
            });

        // add the ExampleConfig menu as a configuration menu
        WysModMenuMod.SetConfigMenu(currentMod.metadata.id, configMenu.Name.Content);
    }
}

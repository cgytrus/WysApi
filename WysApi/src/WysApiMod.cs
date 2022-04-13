using GmmlPatcher;

using UndertaleModLib;
using UndertaleModLib.Models;

using WysApi.Api;

namespace WysApi;

// ReSharper disable once UnusedType.Global
public class WysApiMod : IGameMakerMod {
    // lmaoo you dumb shut up stupid
#pragma warning disable CA1822
    public void Load(int audioGroup, UndertaleData data, ModData currentMod) {
        if(audioGroup != 0) return;
        UndertaleString gameVersion = data.GetMutableGameVersion();
        gameVersion.Content = $"Will You Snail v{gameVersion.Content}\nAPI v{currentMod.metadata.version}";
        Console.WriteLine(gameVersion.Content);
    }
#pragma warning restore CA1822
}

namespace Assets.Scripts.Battle.Manager
{
    /// <summary>
    /// Which arena a battle takes place in. <see cref="None"/> means "fight in the current
    /// scene" (the legacy in-scene flow); any other value maps to a scene to load via
    /// <see cref="ArenaCatalog"/>.
    /// </summary>
    public enum Arena
    {
        None = 0,
        Cave,
        Debug,
        Sandbox,
        Crossing,
    }

    /// <summary>
    /// Single place that maps an <see cref="Arena"/> to its scene name. Add new arenas here
    /// when you add their scenes to Build Settings.
    /// </summary>
    public static class ArenaCatalog
    {
        public static string SceneName(Arena arena)
        {
            switch (arena)
            {
                case Arena.Cave: return "Cave";
                case Arena.Debug: return "DebugScene";
                case Arena.Sandbox: return "SandboxScene";
                case Arena.Crossing: return "Crossing";
                default: return null;
            }
        }

        /// <summary>True if this arena maps to a loadable scene (i.e. not <see cref="Arena.None"/>).</summary>
        public static bool HasScene(Arena arena)
        {
            return !string.IsNullOrEmpty(SceneName(arena));
        }
    }
}

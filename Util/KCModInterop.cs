using System.Reflection;
using UnityEngine;

namespace Porg {
    /// <summary>
    /// A class that enables interoperability between Kingdoms and Castles mods.
    /// </summary>
    /// <remarks>
    /// This class can register a mod with the KCModInterop framework and provide access to other registered mods.
    /// </remarks>
    static class InteropClient {
        /// <summary>
        /// An empty class inheriting from Component. Allows access to the assembly of a mod through the Assembly property of its Type.
        /// </summary>
        private class ModDoorway : MonoBehaviour { }
        /// <summary>
        /// Registers your mod with the KCModInterop framework.
        /// <example>
        /// <code>
        /// ModInterop.Register("ExampleMod");
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="modName">The name with which your mod will be registered. The name must be unique.</param>
        public static void Register(string modName) {
            GameObject mod = new GameObject("KCMod:" + modName); // Creates a empty GameObject to hold your mod's ModDoorway.
            DontDestroyOnLoad(mod);
            mod.AddComponent<ModDoorway>(); // Adds a ModDoorway to the "mod" GameObject.
            GameObject.Instantiate(mod); // Instantiates a copy of the "mod" GameObject.
        }
        /// <summary>
        /// Attemps to load a mod by the name of <paramref name="modName"/>.
        /// <example>
        /// <code>
        /// public void PreScriptLoad(KCModHelper helper) 
        /// { 
        ///     Assembly otherMod; 
        ///     if(!ModInterop.TryGetMod("OtherMod",otherMod)) return; 
        ///     // Do things with the assembly.
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="modName">The name of the mod to load.</param>
        /// <param name="assembly">If the mod is successfully loaded, this will become a reference to its assembly.</param>
        /// <returns>A Boolean value indicating whether the mod was successfully loaded.</returns>
        public static bool TryGetMod(string modName, out Assembly assembly) {
            assembly = default(Assembly); // Must assign a value to the out parameter "assembly" before control leaves the method.
            GameObject mod = GameObject.Find("KCMod:" + modName); // Attempts to find the GameObject of the mod to load.
            if (mod == null) return false; // If the mod is not found, return False to indicate failure.
            /*
            All of GameObject's constructors automatically add a Transform component to the GameObject.
            The ModDoorway component is thus the second component added to the GameObject, making it 1 in a zero-based index system.
            Components are stored in the order they are added, meaning this code will always return the ModDoorway component.
            The CLR considers identical types defined in different assemblies to be different, so you have to get the
            ModDoorway as a Component.
            */
            Component component = mod.GetComponents<Component>()[1];
            // Gets the Assembly of the ModDoorway. GetType() returns the exact type of the object,
            // so in this case you get a Type referring to the other mod's ModDoorway class instead of a Type referring to UnityEngine.Component.
            assembly = component.GetType().Assembly;
            return true; // Indicates the mod was successfully loaded.
        }
    }
}
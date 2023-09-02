using Godot;
using System;
using System.Diagnostics;
using IO = System.IO;
using System.Reflection;

public static class AssemblyResolver {
    
    public static Assembly Assembly;
    const string patchDirectory = "patch";
    
    public static void LoadAssemblies() {
        // Path to current directory
        string dir = IO.Directory.GetCurrentDirectory() + "\\";
        
        // Look for assemblies in patches folder
        foreach (string file in IO.Directory.GetFiles(patchDirectory)) {
            if (!file.ToLower().EndsWith(".dll")) { continue; }
            // Load assemblies
            Assembly = Assembly.LoadFrom(dir + file);
            // Load pck resource
            ProjectSettings.LoadResourcePack((dir + file).Replace(".dll", ".pck"));
        }

        //PrintAssemblyTypes(); // DEBUG
    }
    
    static void PrintAssemblyTypes() {
        if(Assembly == null) {return;}
        
        // Get all the types in the loaded assembly
        Type[] assemblyTypes = Assembly.GetTypes();
        
        // Print all the types
        foreach (Type type in assemblyTypes) {
            string t = type.FullName;
            Debug.Print(t);
        }
    }
}

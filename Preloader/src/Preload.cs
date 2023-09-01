using Godot;
using System;
using System.Diagnostics;
using Godot.Collections;
using Array = Godot.Collections.Array;
using IO = System.IO;

public class Preload : Node {
    [Export] private string mainScenePath = "res://scenes/Main.tscn";

    public override void _Ready() {
        AssemblyResolver.LoadAssemblies();
        LoadScene(mainScenePath, GetTree().Root);
        QueueFree();
    }

    public static void LoadScene(string scenePath, Node parent) {
        var scene = ResourceLoader.Load<PackedScene>(scenePath);
        var patchedScene = LoadPatchedScene(scene);
        var instance = patchedScene.Instance();
        parent.CallDeferred("add_child", instance);
    }

    //Loads a PackedScene but replaces the CScript with an Assembly instantiated version
    private static PackedScene LoadPatchedScene(PackedScene scene) {
        var bundled = scene.Get("_bundled") as Dictionary;
        var variants = bundled["variants"] as Array;

        for(var i = 0; i < variants.Count; i++) {
            if(variants[i] is CSharpScript c) {
                // Get the filename without extension
                string fileNameWithoutExtension = IO.Path.GetFileNameWithoutExtension(c.ResourcePath);

                // If using namespaces, add the namespace name to the string before filename

                try {
                    // Get the type from the loaded assembly
                    Type type = AssemblyResolver.Assembly.GetType(fileNameWithoutExtension);

                    if(type != null) {
                        // Create an instance of the type
                        var instance = Activator.CreateInstance(type) as Node;

                        if(instance != null) {
                            // Change the script of the variant
                            variants[i] = instance.GetScript();
                            instance.Free();
                        }
                        else {
                            GD.Print("Failed to create instance of type: " + fileNameWithoutExtension);
                        }
                    }
                    else {
                        GD.Print("Type not found in the loaded assembly: " + fileNameWithoutExtension);
                    }
                }
                catch(Exception e) {
                    GD.Print("Error loading or creating instance: " + e.Message);
                }
            }
        }

        bundled["variants"] = variants;
        scene.Set("_bundled", bundled);
        Debug.Print("Successfully patched scene!");
        return scene;
    }
}
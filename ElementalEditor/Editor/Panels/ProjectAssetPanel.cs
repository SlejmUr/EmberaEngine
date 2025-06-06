using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Panels
{
    public class ProjectAssetPanel : Panel
    {

        Texture materialTexture;
        Texture checkerTexture;

        int assetCardWidth = 165;
        int assetCardHeight = 220;
        int assetCardPadding = 5;
        int assetCardThumbnailSize = 120;

        int folderTabWidth = 200;
        int directoryTabHeight = 50;
        int assetTilePadding = 10;

        string rootPath;
        string currentPath;

        //List<AssetMetadata> currentPathAssets;
        List<string> directoryContents;

        Dictionary<string, Texture> textureAssetCache;

        public void UpdatePaths()
        {
             //currentPathAssets.Clear();
            directoryContents.Clear();
            foreach (string file in VirtualFileSystem.EnumerateCurrentLevel(currentPath))
            {
                //currentPathAssets.Add(AssetRegistry.GetByPath(file));
            }

            foreach (string dir in Directory.GetDirectories(currentPath))
            {
                directoryContents.Add(Path.GetRelativePath(currentPath, dir));
            }
        }

        public override void OnAttach()
        {
            //currentPathAssets = new List<AssetMetadata>();

            materialTexture = Helper.loadImageAsTex("Editor/Assets/Textures/FileTypeTextures/material.png");
            checkerTexture = Helper.loadImageAsTex("Editor/Assets/Textures/FileTypeTextures/assetCheckerBG.png");

            currentPath = Path.GetFullPath(Path.Combine(editor.projectPath, Project.PROJECT_GAME_FILES_DIRECTORY));
            rootPath = currentPath;

            textureAssetCache = new Dictionary<string, Texture>();
            directoryContents = new List<string>();

            UpdatePaths();
        }

        public override void OnGUI()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

            if (ImGui.Begin("Project Assets"))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(5, 5));

                Vector2 cursorPos = ImGui.GetCursorPos();

                if (ImGui.BeginChild("foldersTab", new Vector2(folderTabWidth, -1), true, ImGuiWindowFlags.AlwaysUseWindowPadding))
                {

                    if (currentPath != rootPath)
                    {
                        if (ImGui.Button("..", new Vector2(-1, 40)))
                        {
                            currentPath = Path.GetFullPath(Path.Combine(currentPath, @".."));
                            UpdatePaths();
                        }
                    }

                    for (int i = 0; i < directoryContents.Count; i++)
                    {
                        if (ImGui.Button(MaterialIconFont.MaterialDesign.Folder + " " + directoryContents[i], new Vector2(-1, 40)))
                        {
                            currentPath = Path.GetFullPath(Path.Combine(currentPath, directoryContents[i]));
                            UpdatePaths();
                        }
                    }

                    ImGui.EndChild();
                }
                ImGui.PopStyleVar();

                ImGui.SetCursorPos(new Vector2(cursorPos.X + folderTabWidth, cursorPos.Y));
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(20, 15));

                if (ImGui.BeginChild("directory_ind", new Vector2(-1, directoryTabHeight), false, ImGuiWindowFlags.AlwaysUseWindowPadding))
                {
                    ImGui.Text(Path.GetRelativePath(editor.projectPath, currentPath));

                    ImGui.EndChild();
                }

                ImGui.PopStyleVar();

                ImGui.SetCursorPos(new Vector2(cursorPos.X + folderTabWidth + assetTilePadding, cursorPos.Y + directoryTabHeight + assetTilePadding));


                //for (int i = 0; i < currentPathAssets.Count; i++)
                //{
                //    //DrawAsset(currentPathAssets[i]);
                //    ImGui.SameLine();
                //}





                ImGui.End();
            }

            ImGui.PopStyleVar();
        }


        //public void DrawAsset(AssetMetadata asset)
        //{
        //    if (asset.AssetType == AssetTypes.EDITOR_TEXTURE_NAME)
        //    {
        //        if (!textureAssetCache.TryGetValue(asset.virtualPath, out Texture tex))
        //        {
        //            tex = AssetLoader.Load<Texture>(asset.virtualPath);
        //            textureAssetCache.Add(asset.virtualPath, tex);
        //        }

        //        DrawAssetTile(tex, Path.GetFileName(asset.virtualPath), asset.AssetType);


        //    } else if (asset.AssetType == AssetTypes.EDITOR_MATERIAL_NAME)
        //    {
        //        DrawAssetTile(materialTexture, Path.GetFileName(asset.virtualPath), asset.AssetType);

        //    } else if (asset.AssetType == AssetTypes.EDITOR_MESH_NAME)
        //    {

        //    } else if (asset.AssetType == AssetTypes.EDITOR_SCENE_NAME)
        //    {

        //    } else if (asset.AssetType == AssetTypes.EDITOR_GENERIC_NAME)
        //    {

        //    }
        //}

        public void DrawAssetTile(Texture thumbnailTexture, string fileName, string fileType)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5f);
            ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 2);

            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.3f, 0.3f, 0.3f, 1f));

            bool isHovered = false;
            bool isActive = false;


            ImGui.BeginChild(fileName, new Vector2(assetCardWidth, assetCardHeight), true, ImGuiWindowFlags.AlwaysUseWindowPadding);

            //ImGui.InvisibleButton(fileName + "_dragRegion", ImGui.GetContentRegionAvail());
            isHovered = ImGui.IsItemHovered();
            isActive = ImGui.IsItemActive();


            float windowWidth = ImGui.GetContentRegionAvail().X;
            float windowHeight = ImGui.GetContentRegionAvail().Y;

            Vector2 checkerPatternSize = new Vector2(windowWidth, assetCardThumbnailSize + (windowWidth - assetCardThumbnailSize) / 2);

            ImGui.Image(checkerTexture.GetRendererID(), checkerPatternSize, new Vector2(0,0), new Vector2(1,0.8f));
            
            ImGui.SetCursorPosX((windowWidth - assetCardThumbnailSize) / 2);
            ImGui.SetCursorPosY((windowWidth - assetCardThumbnailSize) / 2);


            ImGui.Image(thumbnailTexture.GetRendererID(), new Vector2(assetCardThumbnailSize, assetCardThumbnailSize));

            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.3f, 0.3f, 0.3f, 1f));
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(5, 5));
            //ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.1f, 0.1f, 0.1f, 1f));

            ImGui.BeginChild(fileName + "desc", Vector2.One * -1, false, ImGuiWindowFlags.AlwaysUseWindowPadding);

            ImGui.Text(fileName);

            ImGui.EndChild();


            ImGui.PopStyleColor();
            ImGui.PopStyleVar(2);


            if (isActive && ImGui.BeginDragDropSource())
            {
                ImGui.SetDragDropPayload("ASSET_DRAG", IntPtr.Zero, 0);
                ImGui.Text("Dragging: " + fileName);
                ImGui.EndDragDropSource();
            }

            // Draw "Heya" as overlay text
            var drawList = ImGui.GetWindowDrawList();

            // Absolute screen position of the window
            Vector2 windowPos = ImGui.GetWindowPos();

            // Offset within the window (tweak as needed)
            Vector2 overlayPos = new Vector2(windowPos.X + 5, windowPos.Y + 5);

            ImGui.PushFont(editor.interBoldFont);

            // Draw overlay text
            drawList.AddText(overlayPos, ImGui.GetColorU32(new Vector4(0.5f, 0.5f, 0.5f, 1)), fileType);

            ImGui.PopFont();

            ImGui.EndChild();

            ImGui.PopStyleVar(3);
            ImGui.PopStyleColor();
        }
    }
}

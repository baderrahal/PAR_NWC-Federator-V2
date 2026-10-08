using System;
using System.Collections.Generic;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.ComApi;
using Autodesk.Navisworks.Api.Interop.ComApi;
using Federator.Core.Views;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// The saved viewpoints in the open document, F85 and F114: how the tree is counted and
    /// walked, how a view is put in with the hidden state it was saved with, marked, read
    /// back and removed.
    ///
    /// EVERY MEMBER THIS FILE CALLS IS MEASURED. tools\probes\probe-viewpoints.ps1 read
    /// the shape off the installed Autodesk.Navisworks.Api 22.0.0.0 on 2026-09-19,
    /// docs\history\scan.md 5d, and tools\probes\ViewpointProbe measured what a DLL
    /// cannot say on runs the same day and the next, 5j to 5m, and for F114 on 2026-10-05
    /// and 2026-10-07, 5z-h to 5z-z:
    ///
    ///     Document.SavedViewpoints                       is a DocumentSavedViewpoints
    ///     DocumentSavedViewpoints.RootItem               is a FolderItem, a GroupItem
    ///     DocumentSavedViewpoints.AddCopy(GroupItem, SavedItem)   puts a copy in a folder,
    ///         keeping the camera and the overrides of what it copies, 5m
    ///     DocumentSavedViewpoints.Remove(SavedItem)      takes one out, 5k and 5m
    ///     DocumentSavedViewpoints.RemoveAt(GroupItem, int)   takes the child at that index of
    ///         that parent out, one viewpoint two folders deep and nothing else, P13, 5z-u, and
    ///         a folder with every viewpoint under it in the one call, P14, 5z-v
    ///     DocumentSavedViewpoints.AddComment(SavedItem, Comment)   with a comment from
    ///         Document.CreateCommentWithUniqueId, puts a comment on a view two folders deep
    ///         and on a folder that reads back with the same body and author after a save, a
    ///         clear and a reopen, P9, 5z-o. The mark of F114 is written so
    ///     SavedItem.Comments, Comment.Body             read back off a fresh walk, P8 and P9
    ///     SavedViewpoint.Redlines.Size()               the redline count, P8, 5z-n
    ///     SavedItem.Guid                               empty on every item the tool's routes
    ///         make, before and after a save, P10, 5z-p, so the mark carries none
    ///     new FolderItem()                               makes a folder
    ///     new SavedViewpoint(Viewpoint)                  records the camera ALONE, 5j
    ///     DocumentSavedViewpoints.CaptureRuntimeOverrides()       records what is hidden
    ///         and NO camera at all, its Viewpoint throwing Camera not set, 5l. It is used
    ///         here for one thing, reading the hidden state before the writer hides
    ///         anything, and never to write a viewpoint
    ///     InwOpView with ApplyHideAttribs true, the COM API, added to InwOpState.SavedViews
    ///         records BOTH the camera it is given and what is hidden, reads back through
    ///         the .NET API with ContainsVisibilityOverrides true, and presses with both
    ///         after a save and a reopen, 5m. It is the one way found to write a viewpoint
    ///         that opens on its clash with the other disciplines hidden
    ///     Viewpoint.ZoomBox(BoundingBox3D) on a copy of a clash's camera, the box built by
    ///         new BoundingBox3D(Point3D, Point3D), P5, 5z-h, keeps the view direction and puts
    ///         every centre of the box inside the view, P16, 5z-x
    ///     DocumentModels.SetHidden, ResetAllHidden, IsHidden
    ///     DocumentModels.ResetTemporaryMaterials over one collection of every clashing item
    ///         and OverrideTemporaryColor once per colour over a collection record into one
    ///         view where every item reads back its colour, P17, 5z-y
    ///     SavedViewpoint.GetVisibilityOverrides().Hidden read off a capture NOT in the
    ///         tree, 5k, which is how the hidden state is read before the writer hides
    ///         anything and put back after, and off a written view, where each hidden item
    ///         is a model root whose Model.FileName is the hidden model's, P19, 5z-z.
    ///         ResetAllHiddenToModelState is measured too, 5k, and measured to LOSE a hide
    ///         the document held, so it is not called
    ///
    /// EVERY WALK STARTS FROM A FRESH RootItem. AddCopy takes a copy, so a handle read
    /// before it does not see what it put in, which is what SetBuilder learned with the
    /// sets and 5b said not to assume from the pattern. It was read, and the pattern held.
    /// Everything read is disposed on the way, because a wrapper over a document object is
    /// borrowed and never kept.
    /// </summary>
    public static class SavedViewpoints
    {
        /// <summary>
        /// Whether this build knows how to put a viewpoint into the NWF. It went TRUE in
        /// the viewpoints round on 2026-09-19, once 5j had measured that a captured
        /// viewpoint records the hidden state and the run had shown the tree.
        ///
        /// WHY A FLAG AT ALL. A step this tool cannot do is not a step that failed. While
        /// this was false the engine asked for no viewpoints, the judgement was told they
        /// were not requested, and the log said the measurement was outstanding, which is
        /// the opposite of reporting every group FAILED over a feature never attempted.
        /// </summary>
        public static bool CanBuild
        {
            get { return true; }
        }

        /// <summary>
        /// How many saved viewpoints the document holds, walked from the root with every
        /// wrapper disposed on the way, or MINUS ONE where the count could not be taken.
        ///
        /// Minus one is not zero. A rebuild that could not count them does not know whether
        /// they came back, and F50 treats not counted as a reason to leave the NWF on disk
        /// alone, exactly as it treats something lost. Saying zero here would let a rebuild
        /// throw viewpoints away and report that it kept them all.
        /// </summary>
        public static int Count(Document document)
        {
            if (document == null)
            {
                return -1;
            }

            try
            {
                using (GroupItem root = document.SavedViewpoints.RootItem)
                {
                    return CountUnder(root);
                }
            }
            catch (Exception)
            {
                // Swallowed on purpose and reported as not counted. Counting viewpoints is a
                // diagnostic and a diagnostic never stops a run, which is the rule. What it
                // must not do is come back as zero, because zero reads as a real count.
                return -1;
            }
        }

        /// <summary>
        /// The whole tree as one fresh walk reads it, F114, one ViewNode per item in tree
        /// order, for the inventory and the VIEWS TREE block. Each item's comments are read
        /// off SavedItem.Comments, P9, its redlines off SavedViewpoint.Redlines, P8, a folder
        /// having none, row F114-K1, and a viewpoint's camera off its Viewpoint.Position. A
        /// comment list, a redline count or a camera that would not read is counted on the
        /// walk, said by the caller, and the item is handed with that part unread, which the
        /// mark's judge keeps. The Guid is handed as null, P10 having read none on any item
        /// the tool's routes make. Whether a folder held nothing before the run is answered
        /// off the keys of the walk before, ViewNode.EmptyFolderKeys, or false for none.
        /// </summary>
        public static TreeWalk ReadTree(Document document, ICollection<string> emptyBefore)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            TreeWalk walk = new TreeWalk();

            using (GroupItem root = document.SavedViewpoints.RootItem)
            {
                WalkInto(root, new List<string>(), emptyBefore, walk);
            }

            return walk;
        }

        private static void WalkInto(GroupItem parent, List<string> folders, ICollection<string> emptyBefore, TreeWalk walk)
        {
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    string name = child.DisplayName ?? string.Empty;
                    IList<string> comments = CommentBodies(child);

                    if (comments == null)
                    {
                        walk.CommentsNotRead++;
                    }

                    GroupItem group = child as GroupItem;

                    if (group != null)
                    {
                        walk.Nodes.Add(new ViewNode(
                            folders, name, true, i, comments, 0, null, null, ViewNode.HeldNothing(emptyBefore, folders, name)));

                        List<string> under = new List<string>(folders);
                        under.Add(name);
                        WalkInto(group, under, emptyBefore, walk);
                        continue;
                    }

                    SavedViewpoint viewpoint = child as SavedViewpoint;
                    int? redlines = null;
                    Point3 camera = null;

                    if (viewpoint != null)
                    {
                        redlines = RedlineCount(viewpoint);
                        camera = PositionOf(viewpoint);

                        if (redlines == null)
                        {
                            walk.RedlinesNotRead++;
                        }

                        if (camera == null)
                        {
                            walk.CamerasNotRead++;
                        }
                    }
                    else
                    {
                        walk.NeitherKind++;
                    }

                    walk.Nodes.Add(new ViewNode(folders, name, false, i, comments, redlines, camera, null, false));
                }
            }
        }

        /// <summary>Every comment body the item carries, in order, or null where the list would not read.</summary>
        private static IList<string> CommentBodies(SavedItem item)
        {
            try
            {
                List<string> bodies = new List<string>();
                CommentCollection comments = item.Comments;

                if (comments == null)
                {
                    return bodies;
                }

                for (int i = 0; i < comments.Count; i++)
                {
                    using (Comment comment = comments[i])
                    {
                        bodies.Add(comment == null ? string.Empty : comment.Body ?? string.Empty);
                    }
                }

                return bodies;
            }
            catch (Exception)
            {
                // Reported as not read through the walk's count, which the builder writes, and
                // the item is handed with no comment list, which the mark's judge keeps as a
                // person's. A diagnostic read never stops a run.
                return null;
            }
        }

        private static int? RedlineCount(SavedViewpoint viewpoint)
        {
            try
            {
                return viewpoint.Redlines.Size();
            }
            catch (Exception)
            {
                // Counted on the walk and said by the builder. Null is what cannot be proved,
                // which the mark's judge keeps.
                return null;
            }
        }

        private static Point3 PositionOf(SavedViewpoint viewpoint)
        {
            try
            {
                using (Viewpoint recorded = viewpoint.Viewpoint)
                using (Point3D position = recorded.Position)
                {
                    return new Point3(position.X, position.Y, position.Z);
                }
            }
            catch (Exception)
            {
                // Counted on the walk and said by the builder. A camera that will not read is
                // a view the mark's judge keeps.
                return null;
            }
        }

        /// <summary>
        /// Makes every folder on the path that is not there yet, outermost first, each one
        /// re-resolved from a fresh RootItem after the AddCopy that made it, which is the
        /// shape SetBuilder.EnsureFolders measured for the sets. A folder already at its
        /// place is REUSED and never made beside, row F114-K5, so the first run after F114
        /// writes into F85's priority folders rather than twinning them. Returns the depths
        /// of the folders this call made, so the caller marks those and no other.
        /// </summary>
        public static IList<int> EnsureFolders(Document document, IList<string> folders)
        {
            List<int> made = new List<int>();

            if (document == null || folders == null)
            {
                return made;
            }

            for (int depth = 0; depth < folders.Count; depth++)
            {
                using (GroupItem already = ResolveFolders(document, folders, depth + 1))
                {
                    if (already != null)
                    {
                        continue;
                    }
                }

                using (GroupItem parent = ResolveFolders(document, folders, depth))
                {
                    if (parent == null)
                    {
                        throw new InvalidOperationException(
                            "The folder above \"" + folders[depth] + "\" is not there, so it cannot be made.");
                    }

                    using (FolderItem folder = new FolderItem())
                    {
                        folder.DisplayName = folders[depth];
                        document.SavedViewpoints.AddCopy(parent, folder);
                    }
                }

                using (GroupItem now = ResolveFolders(document, folders, depth + 1))
                {
                    if (now == null)
                    {
                        throw new InvalidOperationException(
                            "The folder \"" + folders[depth] + "\" was added and a fresh read does not show it.");
                    }
                }

                made.Add(depth);
            }

            return made;
        }

        /// <summary>
        /// Puts a saved viewpoint with BOTH that camera and what is hidden right now into
        /// that folder path, under that name. MEASURED on 2026-09-20, docs\history\scan.md
        /// 5m, after two .NET routes each recorded half: new SavedViewpoint(Viewpoint)
        /// records the camera and no overrides, 5j, and CaptureRuntimeOverrides records
        /// the overrides and NO camera, its Viewpoint throwing Camera not set, 5l, which
        /// is why the first viewpoints run opened every viewpoint on sky. The COM view is
        /// the one object that carries a flag for it, InwOpView.ApplyHideAttribs, and a
        /// view added with that flag on reads back through the .NET API with
        /// ContainsVisibilityOverrides true and the camera it was given, presses with
        /// both, and keeps both across a save and a reopen.
        ///
        /// BY ONE OF TWO ROUTES, Q59 answered d. throughTheFolder TRUE finds the folder's
        /// OWN InwOpFolderView and adds straight into its SavedViews collection, one tree
        /// operation. throughTheFolder FALSE is the route 5m measured: the COM collection
        /// adds at the ROOT, so the view is copied into the folder with AddCopy, which
        /// keeps both, and the root one is removed, three tree operations. The root one is
        /// found as the LAST root child of that name, because the add appends and a file
        /// may already hold a root item so named. The caller reads the folder copy back
        /// rather than trusting any of it.
        ///
        /// BOTH ARE HERE ON PURPOSE AND THE CHEAP ONE IS NOT ASSUMED BETTER. MEASURED on
        /// 2026-09-20, docs\history\scan.md 5p: over twenty viewpoints both record the
        /// camera, the hidden state, the dimming and the two solid items, all four, and
        /// the cheap one saved three per cent and not the two thirds the operation count
        /// suggests, because what grows is the tree the write walks and not the number of
        /// calls. P18 measured the folder route's add at 6.385 s a view in a tree of 2847
        /// and 0.057 s in a tree of 44, 5z-z, so the tree is what the add costs.
        ///
        /// EACH CALL IS TIMED ON ITS OWN, FR-073, into the seconds handed in: making the
        /// view, finding its COM folder, the add, and on the root route the move.
        /// </summary>
        public static void Record(
            Document document, IList<string> folders, string name, Viewpoint camera, bool throughTheFolder, ViewsSeconds seconds)
        {
            if (document == null || folders == null || string.IsNullOrEmpty(name) || camera == null)
            {
                throw new ArgumentException("A viewpoint needs a document, a folder path, a name and a camera.", "name");
            }

            if (seconds == null)
            {
                throw new ArgumentNullException("seconds");
            }

            InwOpState10 state;
            InwOpView view;

            using (seconds.In(ViewsPart.MakingTheView))
            {
                state = ComApiBridge.State;
                view = (InwOpView)state.ObjectFactory(nwEObjectType.eObjectType_nwOpView, null, null);
                view.name = name;
                view.ApplyHideAttribs = true;

                // Both flags on since the dimming round. ApplyMaterialAttribs is what records
                // that everything but the clashing items is dimmed, MEASURED on 2026-09-20,
                // docs\history\scan.md 5o: with it on the viewpoint carries one material
                // override per item that has a material, and pressing it after a save and a
                // reopen dims them again. With it off the viewpoint carries none, which is
                // what F85 shipped and what Bader could not read.
                view.ApplyMaterialAttribs = true;
                view.anonview = ComApiBridge.ToInwOpAnonView(camera);
            }

            if (throughTheFolder)
            {
                InwOpFolderView folder;

                using (seconds.In(ViewsPart.FindingTheFolder))
                {
                    folder = FindComFolder(state, folders);
                }

                if (folder == null)
                {
                    throw new InvalidOperationException(
                        "The folder path " + string.Join("/", ToArray(folders))
                        + " was made and the COM view of it is not there, so the viewpoint has nowhere to go.");
                }

                using (seconds.In(ViewsPart.AddingTheView))
                {
                    folder.SavedViews().Add(view);
                }

                return;
            }

            using (seconds.In(ViewsPart.AddingTheView))
            {
                state.SavedViews().Add(view);
            }

            using (seconds.In(ViewsPart.MovingIntoTheFolder))
            using (SavedViewpoint atRoot = FindLastAtRoot(document, name))
            {
                if (atRoot == null)
                {
                    throw new InvalidOperationException("The view was added and a fresh read of the root does not show it.");
                }

                using (GroupItem parent = ResolveFolders(document, folders, folders.Count))
                {
                    if (parent == null)
                    {
                        throw new InvalidOperationException(
                            "The folder path " + string.Join("/", ToArray(folders)) + " is not there.");
                    }

                    document.SavedViewpoints.AddCopy(parent, atRoot);
                }

                if (!document.SavedViewpoints.Remove(atRoot))
                {
                    throw new InvalidOperationException("The view was copied into its folder and the root copy would not remove.");
                }
            }
        }

        /// <summary>
        /// How many children of that folder carry that name, are that kind, and carry no
        /// comment with the mark's tag, read fresh. The builder reads it before a view is
        /// recorded, because the view just recorded is found after as the child of its
        /// folder with its name and no mark, P12, and where a person's unmarked view of that
        /// name already sits there the two could not be told apart, P22 unrun, so none is
        /// written there and the person's is left as it is.
        /// </summary>
        public static int CountUnmarked(Document document, IList<string> folders, string name, bool isFolder, ViewpointSettings settings)
        {
            if (document == null || folders == null || name == null)
            {
                return 0;
            }

            using (GroupItem parent = ResolveFolders(document, folders, folders.Count))
            {
                return parent == null ? 0 : UnmarkedChildren(parent, name, isFolder, settings).Count;
            }
        }

        /// <summary>
        /// Writes the mark on the one child of that folder with that name, of that kind and
        /// with no mark yet, the view or folder just made, P12, by AddComment with a comment
        /// from CreateCommentWithUniqueId, P9. Returns the child's index in its folder, which
        /// the read back and the inventory know it by, or minus one with why where there is
        /// not exactly one such child, and then nothing is marked.
        /// </summary>
        public static int Mark(
            Document document, IList<string> folders, string name, bool isFolder, string body, string author,
            ViewpointSettings settings, out string whyNot)
        {
            whyNot = null;

            if (document == null || folders == null || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(body))
            {
                whyNot = "nothing to mark was named";
                return -1;
            }

            using (GroupItem parent = ResolveFolders(document, folders, folders.Count))
            {
                if (parent == null)
                {
                    whyNot = "its folder is not there on a fresh read";
                    return -1;
                }

                IList<int> unmarked = UnmarkedChildren(parent, name, isFolder, settings);

                if (unmarked.Count != 1)
                {
                    whyNot = unmarked.Count == 0
                        ? "a fresh read of its folder shows no unmarked " + (isFolder ? "folder" : "view") + " of its name"
                        : "a fresh read of its folder shows " + unmarked.Count + " unmarked " + (isFolder ? "folders" : "views")
                            + " of its name, so which one was just made is UNKNOWN";
                    return -1;
                }

                SavedItemCollection children = parent.Children;

                using (SavedItem child = children[unmarked[0]])
                using (Comment comment = document.CreateCommentWithUniqueId(body, CommentStatus.New, author ?? string.Empty))
                {
                    document.SavedViewpoints.AddComment(child, comment);
                }

                return unmarked[0];
            }
        }

        /// <summary>The indexes of the parent's children of that name and kind whose comments carry no mark.</summary>
        private static IList<int> UnmarkedChildren(GroupItem parent, string name, bool isFolder, ViewpointSettings settings)
        {
            List<int> found = new List<int>();
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    bool folder = child is GroupItem;

                    if (folder != isFolder || !string.Equals(child.DisplayName, name, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    IList<string> comments = CommentBodies(child);

                    // A comment list that would not read is counted as carrying a mark, because
                    // a view whose mark cannot be seen is not proved the one just made.
                    if (comments == null || ToolViewMark.CarriesAMark(comments, settings))
                    {
                        continue;
                    }

                    found.Add(i);
                }
            }

            return found;
        }

        /// <summary>
        /// What the view at that index of that folder recorded, read off the tree and never
        /// trusted from the write: whether it is there under that name, where its camera sits
        /// against the one asked for, the hidden model roots as their Model.FileName, P19,
        /// how many hidden items are no model root, the material overrides walked once into
        /// a lookup by item, P18, the colour each clashing item will SHOW, the override's
        /// where the view names the item and the item's own where it does not, 5p, and its
        /// comments and redlines for the mark's judge. The first run's tree looked complete
        /// and every viewpoint opened on sky, which is why nothing here is a flag, 5o.
        /// </summary>
        public static ViewReadBack ReadBack(
            Document document,
            IList<string> folders,
            int index,
            string name,
            Viewpoint camera,
            PaintPlan paint,
            ViewpointColour firstColour,
            ViewpointColour secondColour)
        {
            ViewReadBack read = new ViewReadBack();

            if (document == null || folders == null || index < 0 || string.IsNullOrEmpty(name) || camera == null)
            {
                return read;
            }

            using (GroupItem parent = ResolveFolders(document, folders, folders.Count))
            {
                if (parent == null)
                {
                    return read;
                }

                SavedItemCollection children = parent.Children;

                if (index >= children.Count)
                {
                    return read;
                }

                using (SavedItem child = children[index])
                {
                    SavedViewpoint found = child as SavedViewpoint;

                    if (found == null || !string.Equals(child.DisplayName, name, StringComparison.Ordinal))
                    {
                        return read;
                    }

                    read.Found = true;
                    read.Comments = CommentBodies(found);
                    read.Redlines = RedlineCount(found);
                    read.Camera = PositionOf(found);

                    if (read.Camera != null)
                    {
                        using (Point3D asked = camera.Position)
                        {
                            read.CameraDistance = read.Camera.DistanceTo(new Point3(asked.X, asked.Y, asked.Z));
                        }
                    }

                    ReadHidden(found, read);
                    ReadPaint(document, found, paint, firstColour, secondColour, read);
                }
            }

            return read;
        }

        /// <summary>THE COUNT AND NOT THE FLAG, 5o, and each hidden root's model file, P19.</summary>
        private static void ReadHidden(SavedViewpoint found, ViewReadBack read)
        {
            VisibilityOverrides overrides = found.GetVisibilityOverrides();

            if (overrides == null)
            {
                return;
            }

            using (ModelItemCollection hidden = overrides.Hidden)
            {
                if (hidden == null)
                {
                    return;
                }

                read.HiddenCount = hidden.Count;
                List<string> files = new List<string>();

                // The items are enumerated and not disposed one by one, the shape P18 measured,
                // 5z-z, because disposing each item enumerated off a collection threw on every
                // clash of F85's fifth run. The collection is released whole by the using above.
                foreach (ModelItem item in hidden)
                {
                    if (item != null && item.HasModel)
                    {
                        using (Model model = item.Model)
                        {
                            files.Add(model == null ? string.Empty : model.FileName ?? string.Empty);
                        }
                    }
                    else
                    {
                        read.HiddenNotRoots++;
                    }
                }

                read.HiddenFiles = files;
            }
        }

        /// <summary>
        /// The material overrides walked ONCE into a lookup by item path, P18, since a walk
        /// per clashing item over thousands of overrides is the shape that does not finish.
        /// An override with no colour is the dimming, 5z-y. Each clashing item of the plan
        /// then reads the colour it will show, the override's or its own, 5p, and is counted
        /// right or wrong against the colour it was painted, and the painted items handed
        /// to check 4 are every item the view will show in a colour this tool paints.
        /// </summary>
        private static void ReadPaint(
            Document document, SavedViewpoint found, PaintPlan paint, ViewpointColour firstColour, ViewpointColour secondColour, ViewReadBack read)
        {
            Dictionary<ItemPath, ViewpointColour> coloured = new Dictionary<ItemPath, ViewpointColour>();
            AppearanceOverrides overrides = found.GetAppearanceOverrides();

            if (overrides != null && overrides.MaterialOverrides != null)
            {
                read.MaterialOverrideCount = overrides.MaterialOverrides.Count;

                foreach (MaterialOverride material in overrides.MaterialOverrides)
                {
                    Color colour = material.Color;

                    if (colour == null)
                    {
                        continue;
                    }

                    using (ModelItem named = material.Item)
                    {
                        int[] path = PathOf(document, named);

                        if (path == null)
                        {
                            read.ColouredNotPointedAt++;
                            continue;
                        }

                        coloured[new ItemPath(path)] = new ViewpointColour(Clamp(colour.R), Clamp(colour.G), Clamp(colour.B));
                    }
                }
            }

            List<ItemPath> painted = new List<ItemPath>(coloured.Keys);

            if (paint == null || firstColour == null || secondColour == null)
            {
                read.Painted = painted;
                return;
            }

            read.ColoursAsked = paint.Red.Count + paint.Green.Count;
            CountShown(document, paint.Red, firstColour, coloured, painted, read);
            CountShown(document, paint.Green, secondColour, coloured, painted, read);
            read.Painted = painted;
        }

        private static void CountShown(
            Document document,
            IList<ItemPath> items,
            ViewpointColour wanted,
            Dictionary<ItemPath, ViewpointColour> coloured,
            List<ItemPath> painted,
            ViewReadBack read)
        {
            foreach (ItemPath item in items)
            {
                ViewpointColour showing;
                bool recorded = coloured.TryGetValue(item, out showing);

                if (!recorded)
                {
                    showing = OwnColour(document, item);
                }

                if (showing == null)
                {
                    read.ColoursNotRead++;
                    continue;
                }

                if (wanted.Same(showing.Red, showing.Green, showing.Blue))
                {
                    read.ColoursRight++;

                    // An item already the colour it was painted records nothing and still shows
                    // it, 5p, so it is a painted item of the view as a person sees it.
                    if (!recorded)
                    {
                        painted.Add(item);
                    }
                }
                else
                {
                    read.ColoursWrong++;
                }
            }
        }

        /// <summary>The item's own colour off its geometry, or null where it has none or will not read.</summary>
        private static ViewpointColour OwnColour(Document document, ItemPath path)
        {
            try
            {
                using (ModelItem item = ItemAt(document, PathArray(path)))
                {
                    if (item == null || !item.HasGeometry)
                    {
                        return null;
                    }

                    using (ModelGeometry geometry = item.Geometry)
                    {
                        Color own = geometry.OriginalColor;
                        return new ViewpointColour(Clamp(own.R), Clamp(own.G), Clamp(own.B));
                    }
                }
            }
            catch (Exception)
            {
                // Reported as not read through ColoursNotRead, which the builder counts as a
                // view that did not read back. A read back is a diagnostic and never stops a run.
                return null;
            }
        }

        /// <summary>
        /// A colour part the API handed back, held inside 0 to 1 so it can be put in a
        /// ViewpointColour, which refuses anything outside that. Nothing is expected to
        /// be outside it and a value that is would otherwise throw inside a read back.
        /// </summary>
        private static double Clamp(double part)
        {
            if (double.IsNaN(part))
            {
                return 0.0;
            }

            return part < 0.0 ? 0.0 : (part > 1.0 ? 1.0 : part);
        }

        /// <summary>
        /// Takes one item of the inventory out, P13 and P14: the parent resolved fresh by its
        /// folder names, the target re-found by its name, its kind and whether it carries a
        /// mark just before the call, at the index the walk read it at, or among its siblings
        /// where it has moved, and RemoveAt(parent, index). A folder goes with everything
        /// under it in the one call, P14. Nothing is removed where the target is not found
        /// once and once only, and the answer says why.
        /// </summary>
        public static RemovalReadBack RemoveOne(Document document, ViewNode node, ViewpointSettings settings)
        {
            RemovalReadBack answer = new RemovalReadBack();

            if (document == null || node == null)
            {
                answer.WhyNot = "nothing to remove was named";
                return answer;
            }

            using (GroupItem parent = ResolveFolders(document, node.Folders, node.Folders.Count))
            {
                if (parent == null)
                {
                    answer.WhyNot = "its folder is not there on a fresh read";
                    return answer;
                }

                bool marked = ToolViewMark.CarriesAMark(node.Comments, settings);
                SavedItemCollection children = parent.Children;
                answer.CountBefore = children.Count;
                int at = -1;

                if (node.IndexInParent >= 0 && node.IndexInParent < children.Count
                    && IsTheOne(children[node.IndexInParent], node, marked, settings))
                {
                    at = node.IndexInParent;
                }
                else
                {
                    int matches = 0;

                    for (int i = 0; i < children.Count; i++)
                    {
                        if (IsTheOne(children[i], node, marked, settings))
                        {
                            matches++;
                            at = i;
                        }
                    }

                    if (matches != 1)
                    {
                        answer.WhyNot = "it is not at index " + node.IndexInParent + " of its folder any more and "
                            + matches + " of the folder's " + children.Count + " children match its name, kind and mark";
                        return answer;
                    }

                    answer.FoundElsewhere = true;
                }

                document.SavedViewpoints.RemoveAt(parent, at);
                answer.Removed = true;
            }

            using (GroupItem again = ResolveFolders(document, node.Folders, node.Folders.Count))
            {
                answer.CountAfter = again == null ? -1 : again.Children.Count;
            }

            return answer;
        }

        /// <summary>Whether that child is the inventory's item: its name, its kind and whether it carries a mark. The child is released here.</summary>
        private static bool IsTheOne(SavedItem child, ViewNode node, bool marked, ViewpointSettings settings)
        {
            using (child)
            {
                if ((child is GroupItem) != node.IsFolder || !string.Equals(child.DisplayName, node.Name, StringComparison.Ordinal))
                {
                    return false;
                }

                IList<string> comments = CommentBodies(child);
                return comments != null && ToolViewMark.CarriesAMark(comments, settings) == marked;
            }
        }

        /// <summary>
        /// A copy of that camera zoomed to that box, P16: ZoomBox on a copy keeps the view
        /// direction and puts every centre inside the view, through a save, a reopen and a
        /// press, 5z-x. The box is built from the two corners, P5, 5z-h. The caller disposes
        /// what comes back, and the camera handed in is left as it was.
        /// </summary>
        public static Viewpoint Framed(Viewpoint camera, FramingBox box)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("camera");
            }

            if (box == null)
            {
                throw new ArgumentNullException("box");
            }

            Viewpoint framed = camera.CreateCopy();

            try
            {
                using (Point3D low = new Point3D(box.Min.X, box.Min.Y, box.Min.Z))
                using (Point3D high = new Point3D(box.Max.X, box.Max.Y, box.Max.Z))
                using (BoundingBox3D bounds = new BoundingBox3D(low, high))
                {
                    framed.ZoomBox(bounds);
                }
            }
            catch
            {
                framed.Dispose();
                throw;
            }

            return framed;
        }

        /// <summary>
        /// Hides every model whose index is not in the list and shows the rest, so the
        /// viewpoint captured next carries exactly that. The hidden state the document
        /// held before the first call is put back through RestoreHiddenState with the
        /// snapshot the builder took, when the group's writing ends. Each model and each
        /// root read here is a fresh wrapper and is released once its path is copied in.
        /// </summary>
        public static int ShowOnlyModels(Document document, ICollection<int> keep)
        {
            if (document == null || keep == null)
            {
                return 0;
            }

            document.Models.ResetAllHidden();

            using (ModelItemCollection hide = new ModelItemCollection())
            {
                for (int i = 0; i < document.Models.Count; i++)
                {
                    if (keep.Contains(i))
                    {
                        continue;
                    }

                    using (Model model = document.Models[i])
                    using (ModelItem root = model.RootItem)
                    {
                        hide.Add(root);
                    }
                }

                if (hide.Count > 0)
                {
                    document.Models.SetHidden(hide, true);
                }

                return hide.Count;
            }
        }

        /// <summary>
        /// Makes everything transparent except the clashing items of the view, so the
        /// viewpoint recorded next opens the way Clash Detective looks at a clash. Returns
        /// how many items were brought back to solid.
        ///
        /// TWO CALLS AND NOT ONE PER ITEM. The override goes on the model ROOTS and
        /// reaches every leaf under them, and ONE reset goes over the collection of every
        /// clashing item and brings exactly those back, MEASURED on 2026-09-20 over two
        /// items, docs\history\scan.md 5o, and on 2026-10-07 over the 2423 items of one
        /// test in 0.005 s, P17, 5z-y.
        ///
        /// TEMPORARY AND NOT PERMANENT. Both record and both survive a save and a reopen,
        /// 5o. The permanent one writes the transparency onto the item, so it lands in the
        /// NWF if a save arrives before the reset, and its only undo clears every
        /// appearance override the file already held, which nothing can read back first.
        /// That is 5k's trap again and the answer is the same one.
        /// </summary>
        public static int DimAllBut(Document document, double transparency, ICollection<int> shown, ModelItemCollection solid)
        {
            if (document == null)
            {
                return 0;
            }

            // ONLY THE MODELS THIS VIEW SHOWS. A viewpoint records one material override
            // per item it dimmed, measured at 994 for a four model group, and the first
            // dimming run put 33 MB into a 120 KB NWF because it dimmed the models it had
            // just hidden as well. Dimming something already hidden changes no picture and
            // costs a record in every viewpoint.
            using (ModelItemCollection roots = RootsOf(document, shown))
            {
                document.Models.OverrideTemporaryTransparency(roots, transparency);
            }

            if (solid == null || solid.Count == 0)
            {
                return 0;
            }

            document.Models.ResetTemporaryMaterials(solid);
            return solid.Count;
        }

        /// <summary>
        /// Paints every item of the collection one colour in ONE call, P17, 5z-y, red for the
        /// first items and green for the second as Clash Detective paints them, on top of the
        /// dimming that has already left them solid. Returns how many were painted.
        ///
        /// IT IS A TEMPORARY COLOUR AND NOT A PERMANENT ONE, for the same reason the
        /// dimming is, 5o: the permanent one writes onto the item and its only undo
        /// clears every appearance override the file already held.
        ///
        /// IT GOES ON AFTER THE DIMMING AND NEVER BEFORE IT. The transparency override
        /// on the roots reaches every leaf, so painting first and dimming after would
        /// dim the paint straight off again.
        /// </summary>
        public static int PaintMany(Document document, ModelItemCollection items, ViewpointColour colour)
        {
            if (document == null || items == null || items.Count == 0 || colour == null)
            {
                return 0;
            }

            document.Models.OverrideTemporaryColor(items, new Color(colour.Red, colour.Green, colour.Blue));
            return items.Count;
        }

        /// <summary>
        /// The items at those paths resolved into one collection, each released once it is
        /// added, and how many paths resolved nothing counted through the out. The caller
        /// disposes the collection.
        /// </summary>
        public static ModelItemCollection Resolve(Document document, IEnumerable<ItemPath> paths, out int notFound)
        {
            ModelItemCollection items = new ModelItemCollection();
            notFound = 0;

            if (document == null || paths == null)
            {
                return items;
            }

            foreach (ItemPath path in paths)
            {
                using (ModelItem item = ItemAt(document, PathArray(path)))
                {
                    if (item == null)
                    {
                        notFound++;
                        continue;
                    }

                    items.Add(item);
                }
            }

            return items;
        }

        private static int[] PathArray(ItemPath path)
        {
            if (path == null)
            {
                return null;
            }

            int[] array = new int[path.Parts.Count];
            path.Parts.CopyTo(array, 0);
            return array;
        }

        /// <summary>
        /// Takes the dimming off again, scoped to the roots this tool overrode rather than
        /// ResetAllTemporaryMaterials, which would also clear a temporary override this
        /// tool did not set. Measured at under a millisecond, 5o. It takes the PAINT off
        /// too, because a colour override and a transparency override are both temporary
        /// materials and this is the one reset for both, which P18 read off the counts: no
        /// view read a coloured item of the view before, 5z-z.
        /// </summary>
        public static void Undim(Document document)
        {
            if (document == null)
            {
                return;
            }

            using (ModelItemCollection roots = document.Models.CreateCollectionFromRootItems())
            {
                document.Models.ResetTemporaryMaterials(roots);
            }
        }

        /// <summary>
        /// The root items of the models at those indexes, or of every model where the
        /// list is empty or missing. The caller disposes the collection, and each root
        /// read on the way is released once its path is copied in.
        /// </summary>
        private static ModelItemCollection RootsOf(Document document, ICollection<int> shown)
        {
            if (shown == null || shown.Count == 0)
            {
                return document.Models.CreateCollectionFromRootItems();
            }

            ModelItemCollection roots = new ModelItemCollection();

            for (int i = 0; i < document.Models.Count; i++)
            {
                if (!shown.Contains(i))
                {
                    continue;
                }

                using (Model model = document.Models[i])
                using (ModelItem root = model.RootItem)
                {
                    roots.Add(root);
                }
            }

            return roots;
        }

        /// <summary>
        /// The item at that index path, or null. The path is plain ints, so walk one can
        /// name an item and the writer resolve it without keeping a native handle alive
        /// across the group, which is what thousands of handles would be. The caller disposes.
        /// </summary>
        public static ModelItem ItemAt(Document document, int[] path)
        {
            if (document == null || path == null || path.Length == 0)
            {
                return null;
            }

            return document.Models.ResolveIndexPath(path);
        }

        /// <summary>The index path of that item, as plain ints, or null where it has none.</summary>
        public static int[] PathOf(Document document, ModelItem item)
        {
            if (document == null || item == null)
            {
                return null;
            }

            System.Collections.ObjectModel.Collection<int> path = document.Models.CreateIndexPath(item);

            if (path == null || path.Count == 0)
            {
                return null;
            }

            int[] copy = new int[path.Count];
            path.CopyTo(copy, 0);
            return copy;
        }

        /// <summary>
        /// The hidden state the document holds right now, read off a runtime capture that
        /// never goes into the tree, so it can be put back after the writer has hidden and
        /// shown models for every view. MEASURED on 2026-09-19, docs\history\scan.md
        /// 5k: GetVisibilityOverrides().Hidden reads off the un-added capture, and
        /// ResetAllHidden followed by SetHidden on that collection hides the same items
        /// again. ResetAllHiddenToModelState, which this once called instead, ends at the
        /// state the NWC files define and LOSES a hide the document held, measured on the
        /// same run, so it is not called anywhere now. The caller disposes what comes back.
        /// </summary>
        public static HiddenSnapshot SnapshotHidden(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            SavedViewpoint captured = document.SavedViewpoints.CaptureRuntimeOverrides();

            if (captured == null)
            {
                throw new InvalidOperationException("CaptureRuntimeOverrides returned nothing, so the hidden state could not be read.");
            }

            return new HiddenSnapshot(captured);
        }

        /// <summary>
        /// Back to the hidden state the snapshot read: everything shown, then exactly the
        /// items that were hidden hidden again. Returns whether the document reads those
        /// items as hidden afterwards, which is a check and not a trust, and true where
        /// nothing was hidden to begin with.
        /// </summary>
        public static bool RestoreHiddenState(Document document, HiddenSnapshot snapshot)
        {
            if (document == null || snapshot == null)
            {
                return false;
            }

            document.Models.ResetAllHidden();

            if (snapshot.HiddenCount == 0)
            {
                return true;
            }

            document.Models.SetHidden(snapshot.Hidden, true);
            return document.Models.IsHidden(snapshot.Hidden);
        }

        /// <summary>
        /// Walks the folder path from a freshly read RootItem and returns the folder at
        /// that depth, or null when any level is missing. The caller disposes what comes
        /// back. Depth zero is the root itself.
        /// </summary>
        private static GroupItem ResolveFolders(Document document, IList<string> folders, int depth)
        {
            GroupItem current = document.SavedViewpoints.RootItem;

            for (int i = 0; i < depth; i++)
            {
                GroupItem next = FindFolder(current, folders[i]);
                current.Dispose();

                if (next == null)
                {
                    return null;
                }

                current = next;
            }

            return current;
        }

        private static GroupItem FindFolder(GroupItem parent, string folder)
        {
            if (parent == null)
            {
                return null;
            }

            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                SavedItem child = children[i];
                GroupItem group = child as GroupItem;

                if (group != null && string.Equals(child.DisplayName, folder, StringComparison.Ordinal))
                {
                    return group;
                }

                child.Dispose();
            }

            return null;
        }

        /// <summary>
        /// The COM folder view at that path, walked name by name from the COM root
        /// collection, or null where any level is not there. The collection is ONE BASED,
        /// which is the COM convention and not the .NET one.
        ///
        /// The folders themselves are still made through the .NET API by EnsureFolders,
        /// because that is the route already measured. This only finds one that is there.
        /// </summary>
        private static InwOpFolderView FindComFolder(InwOpState10 state, IList<string> folders)
        {
            if (folders == null || folders.Count == 0)
            {
                return null;
            }

            InwSavedViewsColl level = state.SavedViews();
            InwOpFolderView found = null;

            for (int depth = 0; depth < folders.Count; depth++)
            {
                found = FolderNamed(level, folders[depth]);

                if (found == null)
                {
                    return null;
                }

                level = found.SavedViews();
            }

            return found;
        }

        private static InwOpFolderView FolderNamed(InwSavedViewsColl views, string name)
        {
            if (views == null)
            {
                return null;
            }

            for (int i = 1; i <= views.Count; i++)
            {
                InwOpFolderView folder = views[i] as InwOpFolderView;

                if (folder != null && string.Equals(folder.name, name, StringComparison.Ordinal))
                {
                    return folder;
                }
            }

            return null;
        }

        /// <summary>The LAST saved viewpoint of that name at the root, where the COM add appends, or null. The caller disposes it.</summary>
        private static SavedViewpoint FindLastAtRoot(Document document, string name)
        {
            SavedItemCollection items = document.SavedViewpoints.Value;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                SavedItem item = items[i];
                SavedViewpoint viewpoint = item as SavedViewpoint;

                if (viewpoint != null && string.Equals(item.DisplayName, name, StringComparison.Ordinal))
                {
                    return viewpoint;
                }

                item.Dispose();
            }

            return null;
        }

        private static int CountUnder(GroupItem parent)
        {
            if (parent == null)
            {
                return 0;
            }

            int count = 0;
            SavedItemCollection children = parent.Children;

            for (int i = 0; i < children.Count; i++)
            {
                using (SavedItem child = children[i])
                {
                    GroupItem group = child as GroupItem;

                    if (group != null)
                    {
                        count += CountUnder(group);
                    }
                    else
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static string[] ToArray(IList<string> list)
        {
            string[] array = new string[list.Count];
            list.CopyTo(array, 0);
            return array;
        }
    }

    /// <summary>One fresh walk of the saved viewpoints tree, F114, and what on it would not read.</summary>
    public sealed class TreeWalk
    {
        public TreeWalk()
        {
            Nodes = new List<ViewNode>();
        }

        /// <summary>Every item in tree order.</summary>
        public IList<ViewNode> Nodes { get; private set; }

        /// <summary>Items whose comment list would not read, handed with none.</summary>
        public int CommentsNotRead { get; set; }

        /// <summary>Viewpoints whose redline count would not read, handed with null.</summary>
        public int RedlinesNotRead { get; set; }

        /// <summary>Viewpoints whose camera would not read, handed with null.</summary>
        public int CamerasNotRead { get; set; }

        /// <summary>Leaves that are neither a folder nor a saved viewpoint, handed as leaves with nothing read.</summary>
        public int NeitherKind { get; set; }

        /// <summary>How many of the items are viewpoints and not folders.</summary>
        public int Viewpoints
        {
            get
            {
                int count = 0;

                foreach (ViewNode node in Nodes)
                {
                    if (!node.IsFolder)
                    {
                        count++;
                    }
                }

                return count;
            }
        }
    }

    /// <summary>
    /// What a written view recorded, read off the tree. Nothing here is trusted from
    /// the write, and nothing here is a FLAG, because both flags this API offers read true
    /// on a viewpoint that recorded nothing, 5o.
    /// </summary>
    public sealed class ViewReadBack
    {
        /// <summary>Whether a saved viewpoint of that name sits at that index of its folder.</summary>
        public bool Found { get; set; }

        /// <summary>Its comment bodies, or null where they would not read.</summary>
        public IList<string> Comments { get; set; }

        /// <summary>Its redline count, or null where it would not read.</summary>
        public int? Redlines { get; set; }

        /// <summary>Its camera position as recorded, or null where it would not read.</summary>
        public Point3 Camera { get; set; }

        /// <summary>How far its camera sits from the one asked for, in document units, meaningful only where Camera read.</summary>
        public double CameraDistance { get; set; }

        /// <summary>How many items it hides, which is what makes it show its models when pressed.</summary>
        public int HiddenCount { get; set; }

        /// <summary>The model file of each hidden model root, P19, or null where the overrides would not read.</summary>
        public IList<string> HiddenFiles { get; set; }

        /// <summary>Hidden items that are no model root, which a view of this tool never hides.</summary>
        public int HiddenNotRoots { get; set; }

        /// <summary>How many material overrides it carries, the dimming and the paint together.</summary>
        public int MaterialOverrideCount { get; set; }

        /// <summary>Every item the view will show in a painted colour, by its path, for check 4.</summary>
        public IList<ItemPath> Painted { get; set; }

        /// <summary>Overrides carrying a colour whose item could not be pointed at.</summary>
        public int ColouredNotPointedAt { get; set; }

        /// <summary>How many clashing items were painted, the red and the green of the plan.</summary>
        public int ColoursAsked { get; set; }

        /// <summary>How many of them the view will show in the colour they were painted, 5p.</summary>
        public int ColoursRight { get; set; }

        /// <summary>How many will show another colour.</summary>
        public int ColoursWrong { get; set; }

        /// <summary>How many could not be read either way.</summary>
        public int ColoursNotRead { get; set; }
    }

    /// <summary>What one removal came to, P13.</summary>
    public sealed class RemovalReadBack
    {
        public RemovalReadBack()
        {
            CountAfter = -1;
        }

        /// <summary>Whether RemoveAt was called and returned.</summary>
        public bool Removed { get; set; }

        /// <summary>Whether the item had moved from its index and was found once among its siblings.</summary>
        public bool FoundElsewhere { get; set; }

        /// <summary>Why nothing was removed, or null.</summary>
        public string WhyNot { get; set; }

        /// <summary>The folder's children before the call.</summary>
        public int CountBefore { get; set; }

        /// <summary>The folder's children on a fresh read after it, or minus one where the folder was not found again.</summary>
        public int CountAfter { get; set; }
    }

    /// <summary>
    /// What the document had hidden at one moment, held on a runtime capture that is
    /// never put into the tree. The Hidden collection is read once, because the getter
    /// hands out a wrapper, and both are released together.
    /// </summary>
    public sealed class HiddenSnapshot : IDisposable
    {
        private readonly SavedViewpoint captured;
        private readonly ModelItemCollection hidden;

        internal HiddenSnapshot(SavedViewpoint captured)
        {
            this.captured = captured;
            VisibilityOverrides overrides = captured.GetVisibilityOverrides();
            hidden = overrides == null ? null : overrides.Hidden;
        }

        /// <summary>The items that were hidden, or null where the capture carried no overrides.</summary>
        public ModelItemCollection Hidden
        {
            get { return hidden; }
        }

        public int HiddenCount
        {
            get { return hidden == null ? 0 : hidden.Count; }
        }

        public void Dispose()
        {
            if (hidden != null)
            {
                hidden.Dispose();
            }

            captured.Dispose();
        }
    }
}

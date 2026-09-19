namespace Broken
{
    // The shape F52 shipped: a settings object and an outcome object, both called views,
    // in one method scope. CS0128, and the add-in did not build for a day because of it.
    public sealed class DeclaredTwice
    {
        public bool TheFault()
        {
            ViewpointSettings views = new ViewpointSettings();
            ViewpointBuildOutcome views = new ViewpointBuildOutcome(views.Sizes);

            return views.PutAnythingIn;
        }
    }
}

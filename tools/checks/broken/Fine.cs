namespace Broken
{
    // Every shape the check must leave alone. All of it is legal C# and all of it looks
    // close enough to the fault to be worth pinning.
    public sealed class Fine
    {
        public int TwoSiblingScopesMayShareAName(bool which)
        {
            if (which)
            {
                int answer = 1;
                return answer;
            }
            else
            {
                int answer = 2;
                return answer;
            }
        }

        public int TwoLoopsMayShareTheirCounter()
        {
            int total = 0;

            for (int i = 0; i < 3; i++)
            {
                total += i;
            }

            for (int i = 0; i < 3; i++)
            {
                total += i;
            }

            return total;
        }

        public string TwoMethodsMayShareANameAndSoMayABraceInAString()
        {
            string text = "a brace in a string { is text and not a scope";
            char brace = '{';

            return text + brace;
        }

        public string AndThisMethodDeclaresTextToo()
        {
            string text = "the scope of the one above ended";
            return text;
        }
    }
}

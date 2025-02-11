namespace PowerCrypt.Obfuscator.Methods.Counters
{
    public static class Counter
    {
        private static int _transformations = 0;

        public static void Increment()
        {
            _transformations++;
        }

        public static int GetTransformations()
        {
            return _transformations;
        }
    }
}

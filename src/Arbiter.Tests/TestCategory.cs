namespace Arbiter.Tests
{
    /// <summary>
    /// The type of test determined by the type of resouces it depends on.
    /// 
    /// +----------------------+-------+----------------+-------+
    /// |       Feature        | Small |     Medium     | Large |
    /// +----------------------+-------+----------------+-------+
    /// | Network access       | No    | localhost only | Yes   |
    /// | Database             | No    | Yes            | Yes   |
    /// | File system access   | No    | Yes            | Yes   |
    /// | Use external systems | No    | Discouraged    | Yes   |
    /// | Multiple threads     | No    | Yes            | Yes   |
    /// | Sleep statements     | No    | Yes            | Yes   |
    /// | System properties    | No    | Yes            | Yes   |
    /// | Time limit(seconds)  | 60    | 300            | 900+  |
    /// +----------------------+-------+----------------+-------+
    /// 
    /// Source: https://testing.googleblog.com/2010/12/test-sizes.html
    public static class TestCategory
    {
        /// <summary>
        /// An in-process test with no file system access.
        /// </summary>
        public const string Small = "Small";

        /// <summary>
        /// An in-machine test with no network access.
        /// </summary>
        public const string Medium = "Medium";

        /// <summary>
        /// A test with unlimited access.
        /// </summary>
        public const string Large = "Large";
    }
}

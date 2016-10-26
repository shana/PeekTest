using System;
using Microsoft.VisualStudio.Language.Intellisense;

namespace PeekTest
{
    /// <summary>
    /// Represents a PeekRelationship matching CommentPeek. <see cref="IPeekRelationship"/>s
    /// are unique identifiers that apply to a <see cref="IPeekableItemSourceProvider"/>.
    /// </summary>
    public class CommentPeek : IPeekRelationship
    {
        private CommentPeek() { }

        public const string RelationshipName = "CommentPeek";

        public string DisplayName => CommentPeek.RelationshipName;

        public string Name => CommentPeek.RelationshipName;

        public static readonly Lazy<CommentPeek> Instance = new Lazy<CommentPeek>(() => new CommentPeek());
    }
}

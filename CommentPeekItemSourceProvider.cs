using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;

namespace PeekTest
{
    /// <summary>
    /// Exports a <see cref="IPeekableItemSourceProvider"/> for the "code" content type.
    /// </summary>
    [Export(typeof(IPeekableItemSourceProvider))]
    [ContentType("code")]
    [Name("CommentPeek")]
    [SupportsPeekRelationship(CommentPeek.RelationshipName)]
    public class CommentPeekPeekableItemSourceProvider : IPeekableItemSourceProvider
    {
        public IPeekableItemSource TryCreatePeekableItemSource(ITextBuffer textBuffer)
        {
            return new CommentPeekPeekableItemSource(textBuffer);
        }
    }

    /// <summary>
    /// A <see cref="IPeekableItemSource"/> is created for each <see cref="ITextBuffer"/>
    /// in a Visual Studio session.
    /// </summary>
    internal class CommentPeekPeekableItemSource : IPeekableItemSource
    {
        private readonly ITextBuffer textBuffer;

        internal CommentPeekPeekableItemSource(ITextBuffer textBuffer)
        {
            if (textBuffer == null)
            {
                throw new ArgumentNullException(nameof(textBuffer));
            }

            this.textBuffer = textBuffer;
        }

        /// <summary>
        /// Called by a <see cref="IPeekBroker"/> to add <see cref="IPeekableItem"/>s to a current
        /// <see cref="IPeekSession"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="IPeekBroker"/> does not know if the <see cref="IPeekableItemSource"/>
        /// supports the relationship in <see cref="IPeekSession"/>, so you should always check to
        /// see if the relationship actually applies to this class.
        /// </remarks>
        /// <param name="session">The running <see cref="IPeekSession"/>.</param>
        /// <param name="peekableItems">A list of <see cref="IPeekableItem"/>s to append to.</param>
        public void AugmentPeekSession(IPeekSession session, IList<IPeekableItem> peekableItems)
        {
            // Only add a new CommentPeekPeekableItem if the relationship is a CommentPeek
            if (session.RelationshipName.Equals(CommentPeek.RelationshipName, StringComparison.OrdinalIgnoreCase))
            {
                peekableItems.Add(new CommentPeekPeekableItem(textBuffer));
            }
            
        }

        public void Dispose()
        {

        }
    }
}
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Threading;

namespace PeekTest
{
    class ClassDefinitionPeekItem : IPeekableItem
    {
        internal readonly IPeekResultFactory _peekResultFactory;
        internal string _className;
        internal readonly ITextBuffer _textbuffer;
        internal readonly IServiceProvider serviceProvider;

        DTE2 _dte;
        internal DTE2 DTE
        {
            get
            {
                if (_dte == null)
                    _dte = serviceProvider.GetService(typeof(DTE)) as DTE2;

                return _dte;
            }
        }

        public ClassDefinitionPeekItem(IServiceProvider serviceProvider, string className, IPeekResultFactory peekResultFactory, ITextBuffer textbuffer)
        {
            _className = className;
            _peekResultFactory = peekResultFactory;
            _textbuffer = textbuffer;
            this.serviceProvider = serviceProvider;
        }

        public string DisplayName
        {
            // This is unused, and was supposed to have been removed from IPeekableItem.
            get { return null; }
        }

        public IEnumerable<IPeekRelationship> Relationships
        {
            get { yield return PredefinedPeekRelationships.Definitions; }
        }

        public IPeekResultSource GetOrCreateResultSource(string relationshipName)
        {
            return new ClassResultSource(this);
        }
    }

    static class ClassifierDefinitions
    {
        [Export]
        [Name(ContentType)]
        [BaseDefinition(BaseContentType)]
        static ContentTypeDefinition gridContentTypeDefinition;

        [Export]
        [FileExtension(FileExtension)]
        [ContentType(ContentType)]
        static FileExtensionToContentTypeDefinition gridFileExtensionDefinition;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ClassifierName)]
        static ClassificationTypeDefinition typeDefinition;

        public const string ClassifierName = "CommentClassifier";
        public const string FileExtension = ".comment";
        public const string ContentType = "comment";
        public const string BaseContentType = "text";
    }

    [Export(typeof(IPeekableItemSourceProvider))]
    [ContentType(ClassifierDefinitions.ContentType)]
    [Name(ClassifierDefinitions.ClassifierName)]
    [SupportsStandaloneFiles(true)]
    class ClassPeekItemProvider : IPeekableItemSourceProvider
    {
#pragma warning disable 649 // "field never assigned to" -- field is set by MEF.
        [Import]
        private IPeekResultFactory _peekResultFactory;

        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider serviceProvider;
#pragma warning restore 649

        public IPeekableItemSource TryCreatePeekableItemSource(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new ClassPeekItemSource(serviceProvider, textBuffer, _peekResultFactory));
        }
        
    }

    internal sealed class ClassPeekItemSource : IPeekableItemSource
    {
        private readonly ITextBuffer _textBuffer;
        private readonly IPeekResultFactory _peekResultFactory;
        private readonly IServiceProvider serviceProvider;

        public ClassPeekItemSource(IServiceProvider serviceProvider, ITextBuffer textBuffer, IPeekResultFactory peekResultFactory)
        {
            _textBuffer = textBuffer;
            _peekResultFactory = peekResultFactory;
            this.serviceProvider = serviceProvider;
        }

        public void AugmentPeekSession(IPeekSession session, IList<IPeekableItem> peekableItems)
        {
            var triggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);
            if (!triggerPoint.HasValue)
                return;

            peekableItems.Add(new ClassDefinitionPeekItem(serviceProvider, "comment", _peekResultFactory, _textBuffer));
        }

        public void Dispose()
        { }
    }

    class ClassResultSource : IPeekResultSource
    {
        private readonly ClassDefinitionPeekItem peekableItem;

        public ClassResultSource(ClassDefinitionPeekItem peekableItem)
        {
            this.peekableItem = peekableItem;
        }

        public void FindResults(string relationshipName, IPeekResultCollection resultCollection, CancellationToken cancellationToken, IFindPeekResultsCallback callback)
        {
            if (relationshipName != PredefinedPeekRelationships.Definitions.Name)
            {
                return;
            }

            var temp = Path.GetTempFileName();

            using (var displayInfo = new PeekResultDisplayInfo(label: peekableItem._className, labelTooltip: "Comment", title: "Comment title", titleTooltip: "Comment"))
            {
                var result = peekableItem._peekResultFactory.Create
                (
                    displayInfo,
                    temp,
                    new Span(0, 0),
                    0,
                    false
                );

                resultCollection.Add(result);
                callback.ReportProgress(1);
            }
        }
    }
}

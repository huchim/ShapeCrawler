﻿using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using SlideXML.Enums;
using SlideXML.Models.Settings;
using SlideXML.Models.SlideComponents;
using SlideXML.Services;
using SlideXML.Validation;
// ReSharper disable PossibleMultipleEnumeration

namespace SlideXML.Models
{
    /// <summary>
    /// Represents a slide.
    /// </summary>
    public class Slide
    {
        #region Fields

        private readonly SlidePart _xmlSldPart;

        private List<SlideElement> _elements;
        private ImageEx _backgroundImage;

        #region Dependencies

        private readonly IXmlGroupShapeTypeParser _groupShapeTypeParser; // may be better move into _elFactory
        private readonly IBackgroundImageFactory _bgImgFactory;
        private readonly IPreSettings _preSettings;

        #endregion Dependencies

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets slide elements.
        /// </summary>
        public IList<SlideElement> Elements
        {
            get
            {
                if (_elements == null)
                {
                    InitElements();
                }

                return _elements;
            }
        }

        /// <summary>
        /// Returns a slide number in presentation.
        /// </summary>
        public int Number { get; set; } //TODO: Remove public setter somehow

        /// <summary>
        /// Returns a background image of slide.
        /// </summary>
        public ImageEx BackgroundImage
        {
            get
            {
                return _backgroundImage ??= _bgImgFactory.CreateBackgroundSlide(_xmlSldPart);
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="Slide"/> class.
        /// </summary>
        /// TODO: use builder instead public constructor
        public Slide(SlidePart xmlSldPart, int sldNumber, IPreSettings preSettings)
        {
            Check.IsPositive(sldNumber, nameof(sldNumber));
            Number = sldNumber;
            _xmlSldPart = xmlSldPart ?? throw new ArgumentNullException(nameof(xmlSldPart));
            _groupShapeTypeParser = new XmlGroupShapeTypeParser();
            _bgImgFactory = new BackgroundImageFactory();
            _preSettings = preSettings ?? throw new ArgumentNullException(nameof(preSettings));
        }

        #endregion Constructors

        #region Private Methods

        private void InitElements()
        {
            var elCandidates = _groupShapeTypeParser.CreateCandidates(_xmlSldPart.Slide.CommonSlideData.ShapeTree);
            _elements = new List<SlideElement>(elCandidates.Count());
            var elFactory = new ElementFactory(_xmlSldPart);
            foreach (var candidate in elCandidates)
            {
                SlideElement newElement;
                if (candidate.ElementType == ElementType.Group)
                {
                    newElement = elFactory.CreateGroupShape(candidate.CompositeElement, _preSettings);
                }
                else
                {
                    newElement = elFactory.CreateShape(candidate, _preSettings);
                }
                _elements.Add(newElement);
            }
        }

        #endregion Private Methods
    }
}
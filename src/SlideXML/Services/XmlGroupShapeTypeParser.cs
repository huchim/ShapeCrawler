﻿using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using SlideXML.Enums;
using SlideXML.Extensions;
using P = DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;
// ReSharper disable PossibleMultipleEnumeration

namespace SlideXML.Services
{
    /// <summary>
    /// Represents a parser of <see cref="P.ShapeTree"/> and <see cref="P.GroupShape"/> instances.
    /// </summary>
    /// <remarks>
    /// <see cref="P.ShapeTree"/> and <see cref="P.GroupShape"/> both derived from <see cref="P.GroupShapeType"/> class.
    /// </remarks>
    public class XmlGroupShapeTypeParser : IXmlGroupShapeTypeParser
    {
        #region Public Methods

        /// <summary>
        /// Creates candidate collection.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ElementCandidate> CreateCandidates(P.GroupShapeType groupTypeShape, bool groupParsed = true)
        {
            // gets all xml composite elements
            var allCompositeElements = groupTypeShape.Elements<OpenXmlCompositeElement>();

            var supportElements = allCompositeElements.Where(e => e.GetPlaceholderIndex() == null);

            // OLE Objects
            var oleFrames = supportElements.Where(e => e is P.GraphicFrame && e.Descendants<P.OleObject>().Any());
            var oleCandidates = oleFrames.Select(ce => new ElementCandidate
            {
                CompositeElement = ce,
                ElementType = ElementType.OLEObject
            });

            // FILTER PICTURES
            var pictureCandidates = supportElements.Except(oleFrames).Where(e => e is P.Picture || e is P.GraphicFrame && e.Descendants<P.Picture>().Any());
            var graphicFrameImages = pictureCandidates.Where(e => e is P.GraphicFrame).SelectMany(e => e.Descendants<P.Picture>());
            var picAndShapeImages = pictureCandidates.Where(e => e is P.Picture
                                                                 || e is P.Shape && e.Descendants<A.BlipFill>().Any());

            // Picture candidates
            var xmlPictures = graphicFrameImages.Union(picAndShapeImages);
            var picCandidates = xmlPictures.Select(ce => new ElementCandidate
            {
                CompositeElement = ce,
                ElementType = ElementType.Picture
            });

            // Shape candidates
            var xmlShapes = allCompositeElements.Except(pictureCandidates)
                                                                     .Where(e => e is P.Shape);
            var shapeCandidates = xmlShapes.Select(ce => new ElementCandidate
            {
                CompositeElement = ce,
                ElementType = ElementType.AutoShape
            });

            // Table candidates
            var xmlTables = supportElements
                                                            .Except(pictureCandidates)
                                                            .Except(xmlShapes)
                                                            .Where(e => e is P.GraphicFrame grFrame && grFrame.Descendants<A.Table>().Any());
            var tableCandidates = xmlTables.Select(ce => new ElementCandidate
            {
                CompositeElement = ce,
                ElementType = ElementType.Table
            });

            // Chart candidates
            var xmlCharts = supportElements
                                                            .Except(pictureCandidates)
                                                            .Except(xmlShapes)
                                                            .Except(xmlTables)
                                                            .Where(e => e is P.GraphicFrame grFrame && grFrame.HasChart());
            var chartCandidates = xmlCharts.Select(ce => new ElementCandidate
            {
                CompositeElement = ce,
                ElementType = ElementType.Chart
            });

            var allCandidates = picCandidates.Union(shapeCandidates).Union(tableCandidates).Union(chartCandidates).Union(oleCandidates);

            // Group candidates
            if (groupParsed)
            {
                var xmlGroupCandidates = supportElements.Where(e => e is P.GroupShape).Select(ce => new ElementCandidate
                {
                    CompositeElement = ce,
                    ElementType = ElementType.Group
                });
                allCandidates = allCandidates.Union(xmlGroupCandidates);
            }

            return allCandidates;
        }

        #endregion Public Methods
    }
}
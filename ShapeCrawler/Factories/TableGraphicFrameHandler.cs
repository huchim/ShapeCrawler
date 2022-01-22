﻿using System;
using DocumentFormat.OpenXml;
using ShapeCrawler.Settings;
using ShapeCrawler.Shapes;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace ShapeCrawler.Factories
{
    internal class TableGraphicFrameHandler : OpenXmlElementHandler
    {
        private const string Uri = "http://schemas.openxmlformats.org/drawingml/2006/table";
        private readonly ShapeContext.Builder shapeContextBuilder;

        public override IShape Create(OpenXmlCompositeElement pShapeTreeChild, SCSlide slide, SlideGroupShape groupShape)
        {
            if (pShapeTreeChild is P.GraphicFrame pGraphicFrame)
            {
                A.GraphicData graphicData = pGraphicFrame.Graphic.GraphicData;
                if (!graphicData.Uri.Value.Equals(Uri, StringComparison.Ordinal))
                {
                    return this.Successor?.Create(pShapeTreeChild, slide, groupShape);
                }

                var table = new SlideTable(pGraphicFrame, slide, groupShape);

                return table;
            }

            return this.Successor?.Create(pShapeTreeChild, slide, groupShape);
        }
    }
}
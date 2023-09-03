<?xml version="1.0" encoding="UTF-8"?>
<StyledLayerDescriptor xmlns="http://www.opengis.net/sld" xmlns:ogc="http://www.opengis.net/ogc"
                       xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                       xmlns:se="http://www.opengis.net/se"
                       xsi:schemaLocation="http://www.opengis.net/sld http://schemas.opengis.net/sld/1.1.0/StyledLayerDescriptor.xsd"
                       version="1.1.0" xmlns:xlink="http://www.w3.org/1999/xlink">
    <se:Name>polygon_sld</se:Name>
    <se:Description>
        <se:Title>polygon_sld</se:Title>
        <se:Abstract>polygon_sld</se:Abstract>
    </se:Description>
    <NamedLayer>
        <UserStyle>
            <se:FeatureTypeStyle>
                <se:Rule>
                    <se:Description>
                        <se:Title>虚线</se:Title>
                    </se:Description>
                    <se:MinScaleDenominator>100</se:MinScaleDenominator>
                    <se:MaxScaleDenominator>80000</se:MaxScaleDenominator>
                    <se:LineSymbolizer>
                        <se:Stroke>
                            <se:SvgParameter name="stroke">#e0861a</se:SvgParameter>
                            <se:SvgParameter name="stroke-opacity">1</se:SvgParameter>
                            <se:SvgParameter name="stroke-width">2</se:SvgParameter>
                            <se:SvgParameter name="stroke-dasharray">5 5</se:SvgParameter>
                            <se:SvgParameter name="stroke-dashOffset">0</se:SvgParameter>
                        </se:Stroke>
                    </se:LineSymbolizer>
                </se:Rule>
                <se:Rule>
                    <se:Description>
                        <se:Title>Label</se:Title>
                    </se:Description>
                    <se:MinScaleDenominator>100</se:MinScaleDenominator>
                    <se:MaxScaleDenominator>3000</se:MaxScaleDenominator>
                    <se:TextSymbolizer>
                        <se:Label>
                            <ogc:PropertyName>name</ogc:PropertyName>
                        </se:Label>
                        <se:Font>
                            <se:SvgParameter name="font-family">Source Han Sans SC</se:SvgParameter>
                            <se:SvgParameter name="font-size">16</se:SvgParameter>
                            <se:SvgParameter name="font-weight">100</se:SvgParameter>
                            <se:SvgParameter name="font-style">style1</se:SvgParameter>
                        </se:Font>
                        <Fill>
                            <CssParameter name="fill">#FF0000</CssParameter>
                        </Fill>
                    </se:TextSymbolizer>

                </se:Rule>
            </se:FeatureTypeStyle>
        </UserStyle>
    </NamedLayer>
</StyledLayerDescriptor>

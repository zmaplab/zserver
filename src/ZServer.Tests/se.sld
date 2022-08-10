<?xml version="1.0" encoding="UTF-8"?>
<StyledLayerDescriptor xmlns="http://www.opengis.net/sld" xmlns:ogc="http://www.opengis.net/ogc"
                       xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                       xmlns:se="http://www.opengis.net/se"
                       xsi:schemaLocation="http://www.opengis.net/sld http://schemas.opengis.net/sld/1.1.0/StyledLayerDescriptor.xsd"
                       version="1.1.0" xmlns:xlink="http://www.w3.org/1999/xlink">
    <NamedLayer>
        <se:Name>DI_规划道路中线</se:Name>
        <UserStyle>
            <se:Name>DI_规划道路中线</se:Name>
            <se:FeatureTypeStyle>
                <se:Rule>
                    <se:Name>规划道路中线L16</se:Name>
                    <se:Description>
                        <se:Title>规划道路中线L16</se:Title>
                    </se:Description>
                    <ogc:Filter xmlns:ogc="http://www.opengis.net/ogc">
                        <ogc:PropertyIsEqualTo>
                            <ogc:PropertyName>yzdydldj</ogc:PropertyName>
                            <ogc:Literal>次干路</ogc:Literal>
                        </ogc:PropertyIsEqualTo>
                    </ogc:Filter>
                    <se:MinScaleDenominator>6772</se:MinScaleDenominator>
                    <se:MaxScaleDenominator>10000000</se:MaxScaleDenominator>
                    <se:LineSymbolizer>
                        <se:Stroke>
                            <se:SvgParameter name="stroke">#f8f8f8</se:SvgParameter>
                            <se:SvgParameter name="stroke-width">2</se:SvgParameter>
                            <se:SvgParameter name="stroke-linejoin">round</se:SvgParameter>
                            <se:SvgParameter name="stroke-linecap">round</se:SvgParameter>
                        </se:Stroke>
                    </se:LineSymbolizer>
                    <se:LineSymbolizer>
                        <se:Stroke>
                            <se:SvgParameter name="stroke">#dd302d</se:SvgParameter>
                            <se:SvgParameter name="stroke-width">1</se:SvgParameter>
                            <se:SvgParameter name="stroke-linejoin">round</se:SvgParameter>
                            <se:SvgParameter name="stroke-linecap">round</se:SvgParameter>
                            <se:SvgParameter name="stroke-dasharray">4 2</se:SvgParameter>
                        </se:Stroke>
                    </se:LineSymbolizer>
                </se:Rule>
                <se:Rule>
                    <se:Name>规划道路中线L17</se:Name>
                    <se:Description>
                        <se:Title>规划道路中线L17</se:Title>
                    </se:Description>
                    <ogc:Filter xmlns:ogc="http://www.opengis.net/ogc">
                        <ogc:PropertyIsGreaterThan>
                            <ogc:PropertyName>yzdydldj</ogc:PropertyName>
                            <ogc:Literal>次干路</ogc:Literal>
                        </ogc:PropertyIsGreaterThan>
                    </ogc:Filter>
                    <se:MinScaleDenominator>3387</se:MinScaleDenominator>
                    <se:MaxScaleDenominator>6771</se:MaxScaleDenominator>
                    <se:LineSymbolizer>
                        <se:Stroke>
                            <se:SvgParameter name="stroke">#f8f8f8</se:SvgParameter>
                            <se:SvgParameter name="stroke-width">2</se:SvgParameter>
                            <se:SvgParameter name="stroke-linejoin">round</se:SvgParameter>
                            <se:SvgParameter name="stroke-linecap">round</se:SvgParameter>
                        </se:Stroke>
                    </se:LineSymbolizer>
                    <se:LineSymbolizer>
                        <se:Stroke>
                            <se:SvgParameter name="stroke">#dd302d</se:SvgParameter>
                            <se:SvgParameter name="stroke-width">1</se:SvgParameter>
                            <se:SvgParameter name="stroke-linejoin">round</se:SvgParameter>
                            <se:SvgParameter name="stroke-linecap">round</se:SvgParameter>
                            <se:SvgParameter name="stroke-dasharray">4 2</se:SvgParameter>
                        </se:Stroke>
                    </se:LineSymbolizer>
                </se:Rule>
                <se:Rule>
                    <se:Name>规划道路中线L18</se:Name>
                    <se:Description>
                        <se:Title>规划道路中线L18</se:Title>
                    </se:Description>
                    <ogc:Filter xmlns:ogc="http://www.opengis.net/ogc">
                        <ogc:PropertyIsEqualTo>
                            <ogc:PropertyName>yzdydldj</ogc:PropertyName>
                            <ogc:Literal>次干路</ogc:Literal>
                        </ogc:PropertyIsEqualTo>
                    </ogc:Filter>
                    <se:MinScaleDenominator>1694</se:MinScaleDenominator>
                    <se:MaxScaleDenominator>3386</se:MaxScaleDenominator>
                    <se:LineSymbolizer>
                        <se:Stroke>
                            <se:SvgParameter name="stroke">#f8f8f8</se:SvgParameter>
                            <se:SvgParameter name="stroke-width">2</se:SvgParameter>
                            <se:SvgParameter name="stroke-linejoin">round</se:SvgParameter>
                            <se:SvgParameter name="stroke-linecap">round</se:SvgParameter>
                        </se:Stroke>
                    </se:LineSymbolizer>
                    <se:LineSymbolizer>
                        <se:Stroke>
                            <se:SvgParameter name="stroke">#dd302d</se:SvgParameter>
                            <se:SvgParameter name="stroke-width">1</se:SvgParameter>
                            <se:SvgParameter name="stroke-linejoin">round</se:SvgParameter>
                            <se:SvgParameter name="stroke-linecap">round</se:SvgParameter>
                            <se:SvgParameter name="stroke-dasharray">4 2</se:SvgParameter>
                        </se:Stroke>
                    </se:LineSymbolizer>
                </se:Rule>
                <se:Rule>
                    <se:Name>规划道路中线L19</se:Name>
                    <se:Description>
                        <se:Title>规划道路中线L19</se:Title>
                    </se:Description>
                    <ogc:Filter xmlns:ogc="http://www.opengis.net/ogc">
                        <ogc:PropertyIsEqualTo>
                            <ogc:PropertyName>yzdydldj</ogc:PropertyName>
                            <ogc:Literal>次干路</ogc:Literal>
                        </ogc:PropertyIsEqualTo>
                    </ogc:Filter>
                    <se:MinScaleDenominator>847</se:MinScaleDenominator>
                    <se:MaxScaleDenominator>1693</se:MaxScaleDenominator>
                    <se:LineSymbolizer>
                        <se:Stroke>
                            <se:SvgParameter name="stroke">#f8f8f8</se:SvgParameter>
                            <se:SvgParameter name="stroke-width">2</se:SvgParameter>
                            <se:SvgParameter name="stroke-linejoin">round</se:SvgParameter>
                            <se:SvgParameter name="stroke-linecap">round</se:SvgParameter>
                        </se:Stroke>
                    </se:LineSymbolizer>
                    <se:LineSymbolizer>
                        <se:Stroke>
                            <se:SvgParameter name="stroke">#dd302d</se:SvgParameter>
                            <se:SvgParameter name="stroke-width">1</se:SvgParameter>
                            <se:SvgParameter name="stroke-linejoin">round</se:SvgParameter>
                            <se:SvgParameter name="stroke-linecap">round</se:SvgParameter>
                            <se:SvgParameter name="stroke-dasharray">4 2</se:SvgParameter>
                        </se:Stroke>
                    </se:LineSymbolizer>
                </se:Rule>
                <se:Rule>
                    <se:Name>规划道路中线L20</se:Name>
                    <se:Description>
                        <se:Title>规划道路中线L20</se:Title>
                    </se:Description>
                    <ogc:Filter xmlns:ogc="http://www.opengis.net/ogc">
                        <ogc:PropertyIsEqualTo>
                            <ogc:PropertyName>yzdydldj</ogc:PropertyName>
                            <ogc:Literal>次干路</ogc:Literal>
                        </ogc:PropertyIsEqualTo>
                    </ogc:Filter>
                    <se:MinScaleDenominator>100</se:MinScaleDenominator>
                    <se:MaxScaleDenominator>846</se:MaxScaleDenominator>
                    <se:LineSymbolizer>
                        <se:Stroke>
                            <se:SvgParameter name="stroke">#f8f8f8</se:SvgParameter>
                            <se:SvgParameter name="stroke-width">2</se:SvgParameter>
                            <se:SvgParameter name="stroke-linejoin">round</se:SvgParameter>
                            <se:SvgParameter name="stroke-linecap">round</se:SvgParameter>
                        </se:Stroke>
                    </se:LineSymbolizer>
                    <se:LineSymbolizer>
                        <se:Stroke>
                            <se:SvgParameter name="stroke">#dd302d</se:SvgParameter>
                            <se:SvgParameter name="stroke-width">1</se:SvgParameter>
                            <se:SvgParameter name="stroke-linejoin">round</se:SvgParameter>
                            <se:SvgParameter name="stroke-linecap">round</se:SvgParameter>
                            <se:SvgParameter name="stroke-dasharray">4 2</se:SvgParameter>
                        </se:Stroke>
                    </se:LineSymbolizer>
                </se:Rule>


                <se:Rule>
                    <ogc:Filter xmlns:ogc="http://www.opengis.net/ogc">
                        <ogc:PropertyIsEqualTo>
                            <ogc:PropertyName>yzdydldj</ogc:PropertyName>
                            <ogc:Literal>次干路</ogc:Literal>
                        </ogc:PropertyIsEqualTo>
                    </ogc:Filter>
                    <se:MinScaleDenominator>3387</se:MinScaleDenominator>
                    <se:MaxScaleDenominator>8000</se:MaxScaleDenominator>
                    <se:TextSymbolizer>
                        <se:Label>
                            <ogc:PropertyName>dlmc</ogc:PropertyName>
                        </se:Label>
                        <se:Font>
                            <se:SvgParameter name="font-family">微软雅黑</se:SvgParameter>
                            <se:SvgParameter name="font-size">13</se:SvgParameter>
                        </se:Font>
                        <se:LabelPlacement>
                            <se:LinePlacement>
                                <se:GeneralizeLine>true</se:GeneralizeLine>
                            </se:LinePlacement>
                        </se:LabelPlacement>
                        <se:Halo>
                            <se:Radius>2</se:Radius>
                            <se:Fill>
                                <se:SvgParameter name="fill">#ffffff</se:SvgParameter>
                            </se:Fill>
                        </se:Halo>
                        <se:Fill>
                            <se:SvgParameter name="fill">#000000</se:SvgParameter>
                        </se:Fill>
                    </se:TextSymbolizer>
                </se:Rule>
                <se:Rule>
                    <ogc:Filter xmlns:ogc="http://www.opengis.net/ogc">
                        <ogc:PropertyIsEqualTo>
                            <ogc:PropertyName>yzdydldj</ogc:PropertyName>
                            <ogc:Literal>次干路</ogc:Literal>
                        </ogc:PropertyIsEqualTo>
                    </ogc:Filter>
                    <se:MinScaleDenominator>1694</se:MinScaleDenominator>
                    <se:MaxScaleDenominator>3386</se:MaxScaleDenominator>
                    <se:TextSymbolizer>
                        <se:Label>
                            <ogc:PropertyName>dlmc</ogc:PropertyName>
                        </se:Label>
                        <se:Font>
                            <se:SvgParameter name="font-family">微软雅黑</se:SvgParameter>
                            <se:SvgParameter name="font-size">13</se:SvgParameter>
                        </se:Font>
                        <se:LabelPlacement>
                            <se:LinePlacement>
                                <se:GeneralizeLine>true</se:GeneralizeLine>
                            </se:LinePlacement>
                        </se:LabelPlacement>
                        <se:Halo>
                            <se:Radius>2</se:Radius>
                            <se:Fill>
                                <se:SvgParameter name="fill">#ffffff</se:SvgParameter>
                            </se:Fill>
                        </se:Halo>
                        <se:Fill>
                            <se:SvgParameter name="fill">#000000</se:SvgParameter>
                        </se:Fill>
                    </se:TextSymbolizer>
                </se:Rule>
                <se:Rule>
                    <ogc:Filter xmlns:ogc="http://www.opengis.net/ogc">
                        <ogc:PropertyIsEqualTo>
                            <ogc:PropertyName>yzdydldj</ogc:PropertyName>
                            <ogc:Literal>次干路</ogc:Literal>
                        </ogc:PropertyIsEqualTo>
                    </ogc:Filter>
                    <se:MinScaleDenominator>847</se:MinScaleDenominator>
                    <se:MaxScaleDenominator>1693</se:MaxScaleDenominator>
                    <se:TextSymbolizer>
                        <se:Label>
                            <ogc:PropertyName>dlmc</ogc:PropertyName>
                        </se:Label>
                        <se:Font>
                            <se:SvgParameter name="font-family">微软雅黑</se:SvgParameter>
                            <se:SvgParameter name="font-size">15</se:SvgParameter>
                        </se:Font>
                        <se:LabelPlacement>
                            <se:LinePlacement>
                                <se:GeneralizeLine>true</se:GeneralizeLine>
                            </se:LinePlacement>
                        </se:LabelPlacement>
                        <se:Halo>
                            <se:Radius>2</se:Radius>
                            <se:Fill>
                                <se:SvgParameter name="fill">#ffffff</se:SvgParameter>
                            </se:Fill>
                        </se:Halo>
                        <se:Fill>
                            <se:SvgParameter name="fill">#000000</se:SvgParameter>
                        </se:Fill>
                    </se:TextSymbolizer>
                </se:Rule>
                <se:Rule>
                    <ogc:Filter xmlns:ogc="http://www.opengis.net/ogc">
                        <ogc:PropertyIsEqualTo>
                            <ogc:PropertyName>yzdydldj</ogc:PropertyName>
                            <ogc:Literal>次干路</ogc:Literal>
                        </ogc:PropertyIsEqualTo>
                    </ogc:Filter>
                    <se:MinScaleDenominator>100</se:MinScaleDenominator>
                    <se:MaxScaleDenominator>846</se:MaxScaleDenominator>
                    <se:TextSymbolizer>
                        <se:Label>
                            <ogc:PropertyName>dlmc</ogc:PropertyName>
                        </se:Label>
                        <se:Font>
                            <se:SvgParameter name="font-family">微软雅黑</se:SvgParameter>
                            <se:SvgParameter name="font-size">25</se:SvgParameter>
                        </se:Font>
                        <se:LabelPlacement>
                            <se:LinePlacement>
                                <se:GeneralizeLine>true</se:GeneralizeLine>
                            </se:LinePlacement>
                        </se:LabelPlacement>
                        <se:Halo>
                            <se:Radius>2</se:Radius>
                            <se:Fill>
                                <se:SvgParameter name="fill">#ffffff</se:SvgParameter>
                            </se:Fill>
                        </se:Halo>
                        <se:Fill>
                            <se:SvgParameter name="fill">#000000</se:SvgParameter>
                        </se:Fill>
                    </se:TextSymbolizer>
                </se:Rule>


            </se:FeatureTypeStyle>


        </UserStyle>
    </NamedLayer>
    <NamedLayer>
        <se:Name>NAME1</se:Name>
    </NamedLayer>
</StyledLayerDescriptor>

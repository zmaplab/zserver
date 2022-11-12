import Map from "ol/Map";
import TileLayer from "ol/layer/Tile";
import View from "ol/View";
import OSM from "ol/source/OSM";
import { get as getProjection } from "ol/proj";
import { getTopLeft, getWidth } from "ol/extent";
import WMTS from "ol/source/WMTS";
import WMTSTileGrid from "ol/tilegrid/WMTS";
import TileWMS from "ol/source/TileWMS";

const projection = getProjection("EPSG:4326");
const projectionExtent = projection.getExtent();
const size = getWidth(projectionExtent) / 256;
const resolutions = new Array(22);
const matrixIds = new Array(22);
for (let z = 0; z < 22; ++z) {
  // generate resolutions and matrixIds arrays for this WMTS
  resolutions[z] = size / Math.pow(2, z);
  matrixIds[z] = z;
}

const centerXY = [12.477070574987795, 41.85776175318455];

const map = new Map({
  target: "map",
  layers: [
    new TileLayer({
      source: new TileWMS({
        url: "http://localhost:8200/wms",
        params: {
          VERSION: "1.1.1",
          TILED: true,
          LAYERS: "zserver:polygon",
          bordered: true,
        },
        format: "image/png",
        tileGrid: new WMTSTileGrid({
          origin: getTopLeft(projection.getExtent()),
          resolutions: resolutions.splice(1, 20),
          matrixIds: matrixIds.slice(0, 20),
        }),
      }),
    }),
  ],
  view: new View({
    projection: projection,
    center: centerXY,
    zoom: 17,
  }),
});

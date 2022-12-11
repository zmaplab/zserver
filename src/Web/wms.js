import Map from "ol/Map";
import TileLayer from "ol/layer/Tile";
import View from "ol/View";
import OSM from "ol/source/OSM";
import { get as getProjection } from "ol/proj";
import { getTopLeft, getWidth } from "ol/extent";
import WMTS from "ol/source/WMTS";
import WMTSTileGrid from "ol/tilegrid/WMTS";
import TileWMS from "ol/source/TileWMS";

let projection;
let projectionExtent;
let resolutions = new Array(22);
let matrixIds = new Array(22);

projection = getProjection("EPSG:4326");
projectionExtent = projection.getExtent();
let size = getWidth(projectionExtent) / 256;

for (let z = 0; z < 22; ++z) {
  // generate resolutions and matrixIds arrays for this WMTS
  resolutions[z] = size / Math.pow(2, z);
  matrixIds[z] = z;
}

let centerXY = [12.477070574987795, 41.85776175318455];

const searchParams = new URLSearchParams(window.location.search);
const source = searchParams.get("source");
let layer = "zserver:polygon";
debugger;
switch (source) {
  case "3857": {
    layer = "zserver:polygon_3857";
    break;
  }
  case "4326_shp": {
    layer = "zserver:polygon_4326_shp";
    break;
  }
  case "3857_shp": {
    layer = "zserver:polygon_3857_shp";
    // projection = getProjection("EPSG:3857");
    // projectionExtent = projection.getExtent();
    // size = getWidth(projectionExtent) / 256;
    // centerXY = [1388941.1429993785, 5139696.600312929];
    // for (let z = 0; z < 22; ++z) {
    //   // generate resolutions and matrixIds arrays for this WMTS
    //   resolutions[z] = size / Math.pow(2, z);
    //   matrixIds[z] = z;
    // }
    break;
  }
}

const map = new Map({
  target: "map",
  layers: [
    new TileLayer({
      source: new TileWMS({
        url: "http://localhost:8200/wms",
        params: {
          VERSION: "1.1.1",
          TILED: true,
          LAYERS: layer,
          bordered: true,
        },
        format: "image/png",
        tileGrid: new WMTSTileGrid({
          origin: getTopLeft(projection.getExtent()),
          // WMS 的分辩率要从 1 开始
          resolutions: resolutions.splice(1, 20),
          matrixIds: matrixIds.slice(0, 20),
        }),
      }),
    }),
  ],
  view: new View({
    projection: projection,
    center: centerXY,
    zoom: 16,
  }),
});

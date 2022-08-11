import Map from "ol/Map";
import TileLayer from "ol/layer/Tile";
import View from "ol/View";
import OSM from "ol/source/OSM";
import { get as getProjection } from "ol/proj";
import { getTopLeft, getWidth } from "ol/extent";
import WMTS from "ol/source/WMTS";
import WMTSTileGrid from "ol/tilegrid/WMTS";
import proj4 from "./proj4";
import { register } from "ol/proj/proj4";

const x = {
  code: 4490,
  name: "China Geodetic Coordinate System 2000",
  def: "+proj=longlat +ellps=GRS80 +no_defs",
  bbox: [53.56, 73.62, 16.7, 134.77],
};
setProjection(x.code, x.name, x.def, x.bbox);

const projection = getProjection("EPSG:4490");
const projectionExtent = projection.getExtent();
const size = getWidth(projectionExtent) / 256;
const resolutions = new Array(22);
const matrixIds = new Array(22);
for (let z = 0; z < 22; ++z) {
  // generate resolutions and matrixIds arrays for this WMTS
  resolutions[z] = size / Math.pow(2, z);
  matrixIds[z] = z;
}

const projection4326 = getProjection("EPSG:4326");
const projection4326Extent = projection4326.getExtent();
const size4326 = getWidth(projection4326Extent) / 256;
const resolutions4326 = new Array(22);
const matrixIds4326 = new Array(22);
for (let z = 0; z < 22; ++z) {
  // generate resolutions and matrixIds arrays for this WMTS
  resolutions4326[z] = size4326 / Math.pow(2, z);
  matrixIds4326[z] = z;
}
const centerXY = [12.477070574987795, 41.85776175318455];

const map = new Map({
  target: "map",
  layers: [
    // new TileLayer({
    //   source: new OSM(),
    //   opacity: 0.7,
    // }),
    new TileLayer({
      source: new WMTS({
        url: "http://localhost:8200/wmts",
        layer: "zserver:polygon",
        matrixSet: "EPSG:4326",
        format: "image/png",
        projection: projection4326,
        tileGrid: new WMTSTileGrid({
          origin: getTopLeft(projection4326Extent),
          resolutions: resolutions4326.splice(1, 20),
          matrixIds: matrixIds4326.slice(0, 20),
        }),
        style: "default",
        wrapX: true,
      }),
    }),
  ],
  view: new View({
    projection: projection,
    center: centerXY,
    zoom: 12,
  }),
});

function setProjection(code, name, proj4def, bbox) {
  if (code === null || name === null || proj4def === null || bbox === null) {
    return;
  }

  var newProjCode = "EPSG:" + code;
  proj4.defs(newProjCode, proj4def);
  register(proj4);

  var newProj = getProjection(newProjCode);

  var worldExtent = [bbox[1], bbox[2], bbox[3], bbox[0]];
  newProj.setWorldExtent(worldExtent);
  // approximate calculation of projection extent,
  // checking if the world extent crosses the dateline
  if (bbox[1] > bbox[3]) {
    worldExtent = [bbox[1], bbox[2], bbox[3] + 360, bbox[0]];
  }

  // var extent = applyTransform(worldExtent, fromLonLat, undefined, 8);

  newProj.setExtent(worldExtent);
}

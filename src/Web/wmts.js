import Map from 'ol/Map'
import TileLayer from 'ol/layer/Tile'
import View from 'ol/View'
import OSM from 'ol/source/OSM'
import { get as getProjection } from 'ol/proj'
import { getTopLeft, getWidth } from 'ol/extent'
import WMTS from 'ol/source/WMTS'
import XYZ from 'ol/source/XYZ'
import WMTSTileGrid from 'ol/tilegrid/WMTS'
import TileDebug from 'ol/source/TileDebug'
import proj4 from 'proj4'
import { register } from 'ol/proj/proj4'
import { createXYZ } from 'ol/tilegrid'
import { getHeight } from 'ol/extent'

const projection = getProjection('EPSG:4326')
const projectionExtent = projection.getExtent()
var size = getWidth(projectionExtent) / 256
//分辨率
const length = 19
const resolutions = new Array(length)
const matrixIds = new Array(length)
for (let i = 0; i < length; i += 1) {
  const pow = Math.pow(2, i)
  resolutions[i] = size / pow
  matrixIds[i] = i
}
const b = createXYZ()
const projection2 = getProjection('EPSG:3857')
const projectionExtent2 = projection2.getExtent()
const c = resolutionsFromExtent(projectionExtent2)

let centerXY = [121.340165, 29.2364651]
let img_w_url1 =
  'http://t0.tianditu.gov.cn/img_w/wmts?' + 'SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=img&STYLE=default&TILEMATRIXSET=w&FORMAT=tiles' + '&TILEMATRIX={z}&TILEROW={y}&TILECOL={x}&tk='
let img_w_url2 = 'http://localhost:8200/wmts?SERVICE=WMTS&REQUEST=GetTile&version=1.0.0&layer=tianditu_w&tileMatrixSet=EPSG:3857&format=image/png' + '&TILEMATRIX={z}&TILEROW={y}&TILECOL={x}'
let img_w_url3 = 'http://localhost:8200/wmts?SERVICE=WMTS&REQUEST=GetTile&version=1.0.0&layer=cd&tileMatrixSet=EPSG:3857&format=image/png' + '&TILEMATRIX={z}&TILEROW={y}&TILECOL={x}'
const map = new Map({
  target: 'map',
  layers: [
    // new TileLayer({
    //   //瓦片网格数据源
    //   source: new TileDebug({
    //     //投影
    //     projection: 'EPSG:3857',
    //     //获取瓦片网格信息
    //     tileGrid: new WMTSTileGrid({
    //       origin: getTopLeft(projectionExtent),
    //       resolutions: resolutions,
    //       matrixIds: matrixIds,
    //     }),
    //     wrapX: true,
    //   }),
    // }),
    // new TileLayer({
    //   source:
    //     //
    //     new WMTS({
    //       url: 'http://localhost:8200/wmts',
    //       layer: 'tianditu_w',
    //       style: 'default',
    //       matrixSet: 'EPSG:4326',
    //       tileGrid: new WMTSTileGrid({
    //         origin: getTopLeft(projectionExtent),
    //         resolutions: resolutions,
    //         matrixIds: matrixIds,
    //       }),
    //       wrapX: true,
    //     }),
    // }),

    // new TileLayer({
    //   source: new XYZ({
    //     url: img_w_url3,
    //   }),
    // }),
    // new TileLayer({
    //   source: new XYZ({
    //     url: img_w_url2,
    //   }),
    // }),
    // new TileLayer({
    //   source: new XYZ({
    //     url: img_w_url1,
    //   }),
    // }),
    new TileLayer({
      source: new WMTS({
        url: 'https://t0.tianditu.gov.cn/img_c/wmts?&tk=',
        layer: 'img',
        style: 'default',
        matrixSet: 'c',
        version: '1.0.0',
        format: 'tiles',
        tileGrid: new WMTSTileGrid({
          origin: getTopLeft(projectionExtent),
          resolutions: resolutions,
          matrixIds: matrixIds,
        }),
        wrapX: true,
      }),
    }),
    new TileLayer({
      source: new WMTS({
        url: 'https://t0.tianditu.gov.cn/cia_c/wmts?tk=',
        layer: 'cia',
        style: 'default',
        matrixSet: 'c',
        tileGrid: new WMTSTileGrid({
          origin: getTopLeft(projectionExtent),
          resolutions: resolutions,
          matrixIds: matrixIds,
        }),
        wrapX: true,
      }),
    }),
    new TileLayer({
      source: new WMTS({
        url: 'http://localhost:8200/wmts',
        layer: 'cd',
        matrixSet: 'EPSG:4326',
        format: 'image/png',
        tileGrid: new WMTSTileGrid({
          origin: getTopLeft(projectionExtent),
          resolutions: resolutions,
          matrixIds: matrixIds,
        }),
        wrapX: true,
      }),
    }),
  ],
  view: new View({
    projection: projection,
    center: centerXY,
    zoom: 17,
  }),
})

function setProjection(code, name, proj4def, bbox) {
  if (code === null || name === null || proj4def === null || bbox === null) {
    return
  }

  var newProjCode = 'EPSG:' + code
  proj4.defs(newProjCode, proj4def)
  register(proj4)

  var newProj = getProjection(newProjCode)

  var worldExtent = [bbox[1], bbox[2], bbox[3], bbox[0]]
  newProj.setWorldExtent(worldExtent)
  // approximate calculation of projection extent,
  // checking if the world extent crosses the dateline
  if (bbox[1] > bbox[3]) {
    worldExtent = [bbox[1], bbox[2], bbox[3] + 360, bbox[0]]
  }

  // var extent = applyTransform(worldExtent, fromLonLat, undefined, 8);

  newProj.setExtent(worldExtent)
}

function resolutionsFromExtent(extent) {
  const maxZoom = 19

  const height = getHeight(extent)

  const width = getWidth(extent)

  const tileSize = [256, 256]
  const maxResolution = Math.max(width / tileSize[0], height / tileSize[1])

  const length = maxZoom + 1
  const resolutions = new Array(length)
  for (let z = 0; z < length; ++z) {
    resolutions[z] = maxResolution / Math.pow(2, z)
  }
  return resolutions
}

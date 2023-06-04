import 'leaflet/dist/leaflet.css'
import L from 'leaflet'
import 'leaflet.chinatmsproviders'

let map = L.map('map').setView([31.214683, 121.475451], 16)
L.tileLayer('https://t{s}.tianditu.gov.cn/DataServer?T=img_w&X={x}&Y={y}&L={z}&tk={key}', {
  maxZoom: 18,
  subdomains: ['0', '1', '2', '3', '4', '5', '6', '7'],
  attribution: '&copy; <a href="http://www.tianditu.com">Tianditu</a>',
  key: '16158fb1040a55468c8f0612c48bb86e',
}).addTo(map)

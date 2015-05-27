/// <reference path="data.js" />


//var ENV = (function() {
    
//    var localStorage = window.localStorage;

//    return {
//        settings: {
//            /**
//            * state-mgmt
//            */
//            enabled:    localStorage.getItem('enabled')     || 'true',
//            aggressive: localStorage.getItem('aggressive')  || 'false'
//        },
//        toggle: function(key) {
//            var value       = localStorage.getItem(key)
//            newValue    = ((new String(value)) == 'true') ? 'false' : 'true';

//            localStorage.setItem(key, newValue);
//            return newValue;
//        }
//    }
//})()

var myMap = (function ($) {
    "use strict";

    aggressiveEnabled: false;

    var myMap = {},
        map,
        location,
        path,
        aggressiveEnabled,
        locations = [];

    myMap.create = function () {
        // var map = L.map('map').setView([50.505, -5], 13);
        map = L.map('map');


        L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors',
            maxZoom: 18
        }).addTo(map);
        map.on('locationerror', onLocationError);
        map.on('locationfound', onLocationFound);
        map.locate({ setView: true, maxZoom: 16 });
    }

    function onLocationFound(e) {
        var radius = e.accuracy / 2;

        L.marker(e.latlng).addTo(map)
            .bindPopup("You are within " + radius + " meters from this point").openPopup();

        L.circle(e.latlng, radius).addTo(map);
        data.json("SaveLocation", "POST", e.latlng, function () {
            alert('posted OK');
        }
            , true);
    }
    function onLocationError(e) {
        alert(e.message);
    }

    //function watchPosition() {
    //    var fgGeo = window.navigator.geolocation;
    //    if (watchId) {
    //        stopPositionWatch();
    //    }
    //    // Watch foreground location
    //    watchId = fgGeo.watchPosition(function(location) {
    //        setCurrentLocation(location.coords);
    //    }, function() {}, {
    //        enableHighAccuracy: true,
    //        maximumAge: 5000,
    //        frequency: 10000,
    //        timeout: 10000
    //    });
    //};
    //function stopPositionWatch() {
    //    var fgGeo = window.navigator.geolocation;
    //    if (watchId) {
    //        fgGeo.clearWatch(watchId);
    //        watchId = undefined;
    //    }
    //};
    //function setCurrentLocation(location) {
    //    if (location === undefined) {
    //        location = L.marker(e.latlng).addTo(map)
    //        //location = new google.maps.Marker({
    //        //    map: app.map,
    //        //    icon: {
    //        //        path: google.maps.SymbolPath.CIRCLE,
    //        //        scale: 3,
    //        //        fillColor: 'blue',
    //        //        strokeColor: 'blue',
    //        //        strokeWeight: 5
    //        //    }
    //        //});
    //        app.locationAccuracy = new google.maps.Circle({
    //            fillColor: '#3366cc',
    //            fillOpacity: 0.4,
    //            strokeOpacity: 0,
    //            map: app.map
    //        });
    //    }
    //    if (!app.path) {
    //        app.path = new google.maps.Polyline({
    //            map: app.map,
    //            strokeColor: '#3366cc',
    //            fillOpacity: 0.4
    //        });
    //    }
    //    var latlng = new google.maps.LatLng(location.latitude, location.longitude);
        
    //    if (app.previousLocation) {
    //        var prevLocation = app.previousLocation;
    //        // Drop a breadcrumb of where we've been.
    //        app.locations.push(new google.maps.Marker({
    //            icon: {
    //                path: google.maps.SymbolPath.CIRCLE,
    //                scale: 3,
    //                fillColor: 'green',
    //                strokeColor: 'green',
    //                strokeWeight: 5
    //            },
    //            map: app.map,
    //            position: new google.maps.LatLng(prevLocation.latitude, prevLocation.longitude)
    //        }));
    //    }

    //    // Update our current position marker and accuracy bubble.
    //    app.location.setPosition(latlng);
    //    app.locationAccuracy.setCenter(latlng);
    //    app.locationAccuracy.setRadius(location.accuracy);

    //    // Add breadcrumb to current Polyline path.
    //    app.path.getPath().push(latlng);
    //    app.previousLocation = location;
    //}


    return myMap
})(jQuery)

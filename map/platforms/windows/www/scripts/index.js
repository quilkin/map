// For an introduction to the Blank template, see the following documentation:
// http://go.microsoft.com/fwlink/?LinkID=397704
// To debug code on page load in Ripple or on Android devices/emulators: launch your app, set breakpoints, 
// and then run "window.location.reload()" in the JavaScript Console.
(function () {
    "use strict";
    var dateToDisplay = '';
    document.addEventListener( 'deviceready', onDeviceReady.bind( this ), false );
    //document.addEventListener('deviceready', alertDateString(), false);

    function onDeviceReady() {
        // Handle the Cordova pause and resume events
        document.addEventListener( 'pause', onPause.bind( this ), false );
        document.addEventListener( 'resume', onResume.bind( this ), false );
        //checkDateString();
        alertDateString()
        // TODO: Cordova has been loaded. Perform any initialization that requires Cordova here.
        //configureBackgroundGeoLocation();
        //myMap.watchPosition();

    };
    function alertDateString() {
        alert(checkDateString());
    }
    function checkDateString() {
        navigator.globalization.dateToString(
          new Date(),
          //function (date) { alert('date: ' + date.value + '\n'); },
          function (date) { dateToDisplay = date.value; },
          function () { dateToDisplay = 'Error getting dateString\n'; },
          { formatLength: 'short', selector: 'date and time' }
        );
        return dateToDisplay;
    }

    $(document).ready(function () {
        myMap.create();
 
    });

    function onPause() {
        /**
        * Cordova foreground geolocation watch has no stop/start detection or scaled distance-filtering to conserve HTTP requests based upon speed.  
        * You can't leave Cordova's GeoLocation running in background or it'll kill your battery.  This is the purpose of BackgroundGeoLocation:  to intelligently 
        * determine start/stop of device.
        */
            console.log('- onPause');
            //myMap.stopPositionWatch();
    };

    function onResume() {
        /**
        * Once in foreground, re-engage foreground geolocation watch with standard Cordova GeoLocation api
        */
            console.log('- onResume');
           // myMap.watchPosition();
    };


    //function configureBackgroundGeoLocation() {
    //    var fgGeo = window.navigator.geolocation,
    //        bgGeo = window.plugins.backgroundGeoLocation;

    //    // app.onClickHome();

    //    /**
    //    * This would be your own callback for Ajax-requests after POSTing background geolocation to your server.
    //    */
    //    var yourAjaxCallback = function (response) {
    //        bgGeo.finish();
    //    };

    //    /**
    //    * This callback will be executed every time a geolocation is recorded in the background.
    //    */
    //    var callbackFn = function (location) {
    //        console.log('[js] BackgroundGeoLocation callback:  ' + location.latitude + ',' + location.longitude);

    //        // Update our current-position marker.
    //        setCurrentLocation(location);

    //        // After you Ajax callback is complete, you MUST signal to the native code, which is running a background-thread, that you're done and it can gracefully kill that thread.
    //        yourAjaxCallback.call(this);
    //    };

    //    var failureFn = function (error) {
    //        console.log('BackgroundGeoLocation error');
    //    };

    //    // Only ios emits this stationary event
    //    bgGeo.onStationary(function (location) {
    //        //if (!app.stationaryRadius) {
    //        //    app.stationaryRadius = new google.maps.Circle({
    //        //        fillColor: '#cc0000',
    //        //        fillOpacity: 0.4,
    //        //        strokeOpacity: 0,
    //        //        map: app.map
    //        //    });
    //        //}
    //        //var radius = (location.accuracy < location.radius) ? location.radius : location.accuracy;
    //        //var center = new google.maps.LatLng(location.latitude, location.longitude);
    //        //app.stationaryRadius.setRadius(radius);
    //        //app.stationaryRadius.setCenter(center);

    //    });

    //    // BackgroundGeoLocation is highly configurable.
    //    bgGeo.configure(callbackFn, failureFn, {
    //        url: 'http://www.quilkin.co.uk/service1.svc/SaveLocation', // <-- Android ONLY:  your server url to send locations to
    //        params: {
    //            auth_token: 'user_secret_auth_token',    //  <-- Android ONLY:  HTTP POST params sent to your server when persisting locations.
    //            foo: 'bar'                              //  <-- Android ONLY:  HTTP POST params sent to your server when persisting locations.
    //        },
    //        desiredAccuracy: 0,
    //        stationaryRadius: 50,
    //        distanceFilter: 50,
    //        notificationTitle: 'Background tracking', // <-- android only, customize the title of the notification
    //        notificationText: 'ENABLED', // <-- android only, customize the text of the notification
    //        activityType: 'AutomotiveNavigation',
    //        debug: true, // <-- enable this hear sounds for background-geolocation life-cycle.
    //        stopOnTerminate: false // <-- enable this to clear background location settings when the app terminates
    //    });

    //    // Turn ON the background-geolocation system.  The user will be tracked whenever they suspend the app.
    //    var settings = ENV.settings;

    //    if (settings.enabled == 'true') {
    //        bgGeo.start();

    //        if (settings.aggressive == 'true') {
    //            bgGeo.changePace(true);
    //        }
    //    }
    //};
} )();
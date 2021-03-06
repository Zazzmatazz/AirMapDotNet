﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web;
using AirMapDotNet.Entities;
using AirMapDotNet.Entities.FlightAPI;
using AirMapDotNet.Entities.GeoJSON;
using AirMapDotNet.Entities.GeoJSON.GeoObjects;
using Newtonsoft.Json;

namespace AirMapDotNet.Services
{
    /// <summary>
    /// Exposes several methods for the Flight API.
    /// </summary>
    internal class FlightService : AirMapService
    {
        internal FlightService(AirMap am)
            : base(am)
        { }

        /// <summary>
        /// The default enhancement state for requests.
        /// </summary>
        private const bool DefaultEnhance = false;

        /// <summary>
        /// Retrieves flight by its ID.
        /// </summary>
        /// <param name="id">The flight's unique ID.</param>
        /// <param name="enhance">If <b>true</b>, the response will populate detailed fields such as <i>aircraft</i> or <i>pilot</i>.</param>
        /// <returns>The flight with the ID <paramref name="id"/>.</returns>
        /// <exception cref="AirMapException">If the request fails.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is null.</exception>
        internal async Task<Flight> GetFlight(string id, bool enhance)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            Href<Flight> flightLink = new Href<Flight>(new Uri(string.Format(AirMap_Flight_ByID, id) + (enhance ? "?enhance=true" : "")));
            
            return await AirMap.GetAsync(flightLink);
        }

        /// <summary>
        /// Retrieves a list of all flights within a geographic area.
        /// </summary>
        /// <param name="geom">A bounding geometry.</param>
        /// <param name="limit">The maximum amount of flights to retrieve in a request.</param>
        /// <returns>A list of all flights inside the area defined in <paramref name="geom"/>.</returns>
        /// <exception cref="AirMapException">If the request fails.</exception>
        /// <exception cref="AirMapException">If the serialization of <paramref name="geom"/> fails.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="geom"/> is null.</exception>
        internal async Task<IEnumerable<Flight>> GetFlights(Geometry geom, int limit)
        {
            if (geom == null)
                throw new ArgumentNullException(nameof(geom));

            string geoJson = JsonConvert.SerializeObject(geom);

            return await GetFlights(geoJson, limit, DefaultEnhance);
        }

        /// <summary>
        /// Retrieves a list of all flights within a geographic area.
        /// </summary>
        /// <param name="geoJson">A description of the area to request in <a href="http://geojson.org/">GeoJSON</a> format.</param>
        /// <param name="limit">The maximum amount of flights to retrieve in a request.</param>
        /// <param name="enhance">If <b>true</b>, the response will populate detailed fields such as <i>aircraft</i> or <i>pilot</i>.</param>
        /// <returns>A list of all flights inside the area defined in <paramref name="geoJson"/>.</returns>
        /// <exception cref="AirMapException">If the request fails.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="geoJson"/> is null or equals <see cref="string.Empty"/>.</exception>
        internal async Task<IEnumerable<Flight>> GetFlights(string geoJson, int limit, bool enhance)
        {
            if (string.IsNullOrEmpty(geoJson))
                throw new ArgumentNullException(nameof(geoJson));

            // Remove the excess crap from the GeoJSON string.
            geoJson = geoJson.Replace("\r", "");
            geoJson = geoJson.Replace("\n", "");
            geoJson = geoJson.Replace(" ", "");

            string geoJsonEncoded = HttpUtility.UrlEncode(geoJson);

            Href<PagedEntityCollection<Flight>> flightLink = new Href<PagedEntityCollection<Flight>>(new Uri(AirMap_Flight_All));

            NameValueCollection parms = new NameValueCollection
            {
                ["geometry"] = geoJsonEncoded,
                ["enhance"] = enhance ? "true" : "false",
                ["limit"] = limit.ToString()
            };

            PagedEntityCollection<Flight> flightCollection = await AirMap.GetAsync(flightLink, parms);

            return flightCollection;
        }

        /// <summary>
        /// Retrieves a list of all currently active flights within a geographic area.
        /// </summary>
        /// <param name="geom">The geographic area to search.</param>
        /// <param name="limit">The maximum amount of flights to return.</param>
        /// <param name="enhance">If <b>true</b>, the returned data will be enhanced with extra information.</param>
        /// <returns>A list of all currently active flights in the area defined by <paramref name="geom"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="geom"/> is null.</exception>
        /// <exception cref="AirMapException">If the request fails.</exception>
        internal async Task<IEnumerable<Flight>> GetCurrentFlights(Geometry geom, int limit, bool enhance)
        {
            if (geom == null)
                throw new ArgumentNullException(nameof(geom));

            Href<PagedEntityCollection<Flight>> flightLink = new Href<PagedEntityCollection<Flight>>(new Uri(AirMap_Flight_All));

            string geoJsonEncoded = HttpUtility.UrlEncode(JsonConvert.SerializeObject(geom));
            NameValueCollection parms = new NameValueCollection
            {
                ["start_before"] = DateTime.UtcNow.ToString("O"),
                ["end_after"] = DateTime.UtcNow.ToString("O"),
                ["geometry"] = geoJsonEncoded,
                ["enhance"] = enhance ? "true" : "false",
                ["limit"] = limit.ToString()
            };

            PagedEntityCollection<Flight> flightCollection = await AirMap.GetAsync(flightLink, parms);

            return flightCollection;
        }

        /// <summary>
        /// Retrieves a list of all flights created by the currently authenticated user.
        /// </summary>
        /// <param name="limit">The maximum amount of flights to return.</param>
        /// <param name="enhance">If <b>true</b>, the returned data will be enhanced with extra information.</param>
        /// <returns>A list of all flights created by the currently authenticated user.</returns>
        /// <exception cref="AuthenticationException">If the <see cref="AirMap.AuthenticationToken"/> is not set, or has expired, or the token is not valid for this resource.</exception>
        /// <exception cref="AirMapException">If the request fails.</exception>
        internal async Task<IEnumerable<Flight>> GetMyFlights(int limit, bool enhance)
        {
            if (AirMap.AuthenticationToken == null)
                throw new AuthenticationException("Authentication token not set.");
            if (!AirMap.AuthenticationToken.IsValid)
                throw new AuthenticationException("Authentication token has expired.");

            Href<PagedEntityCollection<Flight>> flightLink = new Href<PagedEntityCollection<Flight>>(new Uri(AirMap_Flight_All));
            
            NameValueCollection parms = new NameValueCollection
            {
                ["pilot_id"] = AirMap.AuthenticationToken.User.UserID,
                ["enhance"] = enhance ? "true" : "false",
                ["limit"] = limit.ToString()
            };

            PagedEntityCollection<Flight> flightCollection = await AirMap.GetAsync(flightLink, parms);

            return flightCollection;
        }

        /// <summary>
        /// Creates a new flight using the parameters in <see cref="FlightCreationParameters"/>.
        /// </summary>
        /// <param name="creationParams">The parameters of the new flight.</param>
        /// <returns>The created flight.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AuthenticationException">If the <see cref="AirMap.AuthenticationToken"/> is not set, or has expired, or the token is not valid for this resource.</exception>
        /// <exception cref="AirMapException">If the <see cref="FlightCreationParameters.Geometry"/> property of <paramref name="creationParams"/> is not a <see cref="LineString"/> or <see cref="Polygon"/>.</exception>
        /// <exception cref="AirMapException">If the request fails.</exception>
        internal async Task<Flight> CreateFlight(FlightCreationParameters creationParams)
        {
            if (creationParams == null)
                throw new ArgumentNullException(nameof(creationParams));
            if (AirMap.AuthenticationToken == null)
                throw new AuthenticationException("Authentication token not set.");
            if (!AirMap.AuthenticationToken.IsValid)
                throw new AuthenticationException("Authentication token has expired.");

            Href<Flight> createFlightHref = new Href<Flight>(new Uri(AirMap_Flight_Point));

            if (creationParams.Geometry != null)
            {
                if (creationParams.Geometry.GeometryObject is LineString)
                    createFlightHref = new Href<Flight>(new Uri(AirMap_Flight_Path));
                else if (creationParams.Geometry.GeometryObject is Polygon)
                    createFlightHref = new Href<Flight>(new Uri(AirMap_Flight_Polygon));
                else
                    throw new AirMapException("The only accepted geometries are LineString and Polygon!");
            }

            return await AirMap.PostAsync(createFlightHref, creationParams);
        }

        /// <summary>
        /// Deletes a flight by its unique ID.
        /// </summary>
        /// <param name="flightId">The flight's unique ID.</param>
        /// <returns><b>True</b> if the deletion was successful.</returns>
        /// <exception cref="AirMapException">If the request fails.</exception>
        /// <exception cref="AuthenticationException">If the <see cref="AirMap.AuthenticationToken"/> is not set, or has expired, or the token is not valid for this resource.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="flightId"/> is null.</exception>
        internal async Task<bool> DeleteFlight(string flightId)
        {
            if (string.IsNullOrEmpty(flightId))
                throw new ArgumentNullException(nameof(flightId));
            if (AirMap.AuthenticationToken == null)
                throw new AuthenticationException("Authentication token not set.");
            if (!AirMap.AuthenticationToken.IsValid)
                throw new AuthenticationException("Authentication token has expired.");


            Href<EmptyEntity> deleteHref = new Href<EmptyEntity>(new Uri(string.Format(AirMap_Flight_DeleteByID, flightId)));

            // Post an empty object.
            try
            {
                await AirMap.PostAsync(deleteHref, new object());
                return true;
            } catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ends a flight by its unique ID.
        /// </summary>
        /// <param name="flightId">The flight's unique ID.</param>
        /// <returns>The time the flight was ended.</returns>
        /// <exception cref="AirMapException">If the request fails.</exception>
        /// <exception cref="AuthenticationException">If the <see cref="AirMap.AuthenticationToken"/> is not set, or has expired, or the token is not valid for this resource.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="flightId"/> is null.</exception>
        internal async Task<DateTime> EndFlight(string flightId)
        {
            if (string.IsNullOrEmpty(flightId))
                throw new ArgumentNullException(nameof(flightId));
            if (AirMap.AuthenticationToken == null)
                throw new AuthenticationException("Authentication token not set.");
            if (!AirMap.AuthenticationToken.IsValid)
                throw new AuthenticationException("Authentication token has expired.");

            Href<FlightEndParameters> deleteHref = new Href<FlightEndParameters>(new Uri(string.Format(AirMap_Flight_EndByID, flightId)));

            // Post an empty object.
            FlightEndParameters fep = await AirMap.PostAsync(deleteHref, new object());

            return fep.EndTime;
        }
    }
}
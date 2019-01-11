using System;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

namespace Adrenak.Xavier.Specialized {
	public abstract class Transit {
		/// <summary>
		/// Invoked every time a transmission is received
		/// </summary>
		public event Action<Transmission> OnGetTransmission;

		/// <summary>
		/// Handler for responding to incoming requests
		/// </summary>
		/// <param name="request">The parameters provided with the request</param>
		/// <returns>The result to be sent to the requester</returns>
		public delegate object Responder(object request);

		/// <summary>
		/// Handles the response of a request
		/// </summary>
		/// <param name="response">The object returned as request</param>
		public delegate void Requester(object response);

		/// <summary>
		/// Subscription to networked events
		/// </summary>
		/// <param name="obj">The parameters/payload provided with the event published</param>
		public delegate void Subscription(object obj);

		protected Dictionary<string, Responder> m_Responders = new Dictionary<string, Responder>();
		protected Dictionary<string, List<Subscription>> m_Subscriptions = new Dictionary<string, List<Subscription>>();

		public Node Node { get; private set; }

		protected Transit(Node.Mode mode) {
			Node = Node.New(mode);
			Node.OnGetMessage += delegate (Message message) {
				if (message.eventType == EventType.Data)
					HandleMessage(message);
			};
		}

		// ================================================
		// MESSAGE HANDLING
		// ================================================
		protected void HandleMessage(Message message) {
			var data = message.data;
			var str = Encoding.UTF8.GetString(data);
			var transmission = JsonUtility.FromJson<Transmission>(str);

			if (transmission == null) return;
			if (OnGetTransmission != null) OnGetTransmission(transmission);

			var payload = Utils.ByteArrayToObject(transmission.bytes);

			switch (transmission.id) {
				case Glossary.k_RequestTag:
					DispatchRequest((Invocation)payload);
					break;
				default:
					DispatchSubscription(transmission.id, payload);
					break;
			}
		}

		void DispatchSubscription(string name, object obj) {
			if (!m_Subscriptions.ContainsKey(name)) return;
			foreach (var listener in m_Subscriptions[name])
				listener(obj);
		}

		void DispatchRequest(Invocation request) {
			if (!m_Responders.ContainsKey(request.method)) return;
			var response = m_Responders[request.method](request.obj);

			Publish(
				Glossary.k_RespondTag,
				new Invocation() {
					method = request.method,
					invokeID = request.invokeID,
					obj = response
				}
			);
		}

		// ================================================
		// NETWORKED EVENT PUBLISHING
		// ================================================
		/// <summary>
		/// Publish an event without payload
		/// </summary>
		/// <param name="name">The name of the event</param>
		/// <param name="id">For Server Transit: The connected ID to be dispatched to</param>
		/// <returns>Whether the event was published over the network</returns>
		public bool Publish(string name, int id = -1) {
			return Publish(name, null);
		}

		/// <summary>
		/// Publish an event with a name and an object payload
		/// </summary>
		/// <param name="name">The name of the event</param>
		/// <param name="id">For Server Transit: The connected ID to be dispatched to</param>
		/// <param name="obj">The object to be sent as payload</param>
		/// <returns>Whether the event was published over the network</returns>
		public bool Publish(string name, object obj, int id = -1) {
			return Publish(name, Utils.ObjectToByteArray(obj), id);
		}

		public abstract bool Publish(string name, byte[] bytes, int id);

		// ================================================
		// NETWORKED EVENT SUBSCRIBING
		// ================================================
		/// <summary>
		/// Subscirbe to an event using the name
		/// </summary>
		/// <param name="name">The name of the event</param>
		/// <param name="subscription">The subscription handler</param>
		public void Subscribe(string name, Subscription subscription) {
			if (!m_Subscriptions.ContainsKey(name))
				m_Subscriptions[name] = new List<Subscription>();

			m_Subscriptions[name].Add(subscription);
		}

		/// <summary>
		/// Unsubscribe from an event 
		/// </summary>
		/// <param name="name">The name of the event</param>
		/// <param name="listener">The handler to be unsubscribed</param>
		public void Unsubscribe(string name, Subscription listener) {
			if (!m_Subscriptions.ContainsKey(name))
				return;

			m_Subscriptions[name].Remove(listener);
		}

		// ================================================
		// NETWORKED REQUESTS
		// ================================================
		/// <summary>
		/// Invoke a networked method request
		/// </summary>
		/// <param name="method">The name of the method</param>
		/// <param name="requestback">The handler for the request result</param>
		/// <returns>Whether the request was successfully made</returns>
		public bool Request(string method, Requester requestback) {
			return Request(method, null, requestback);
		}

		/// <summary>
		/// Invoke a networked method request with a payload object
		/// </summary>
		/// <param name="method">The name of the method</param>
		/// <param name="obj">The payload object</param>
		/// <param name="requestback">The handler for the request result</param>
		/// <returns>Whether the request was successfully made</returns>
		public bool Request(string method, object obj, Requester requestback) {
			var invokeID = Guid.NewGuid().ToString();

			Subscribe(Glossary.k_RespondTag, responseObj => {
				var response = (Invocation)responseObj;
				if (response.invokeID.Equals(invokeID))
					requestback(response.obj);
			});

			return Publish(
				Glossary.k_RequestTag,
				new Invocation() {
					method = method,
					invokeID = invokeID,
					obj = obj
				}
			);
		}

		// ================================================
		// NETWORKED EVENT RESPONSES
		// ================================================
		/// <summary>
		/// Return the result for a networked request
		/// </summary>
		/// <param name="method">The method name of the networked  request</param>
		/// <param name="responder">The handler for the request</param>
		public void Respond(string method, Responder responder) {
			m_Responders[method] = responder;
		}
	}
}

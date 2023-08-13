using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using DG.Tweening;

public class ClientManager : MonoBehaviour
{
    public string apiUrl = "https://qa2.sunbasedata.com/sunbase/portal/api/assignment.jsp?cmd=client_data";
    public Text clientListText;
    public Dropdown filterDropdown;

    private List<Client> allClients = new List<Client>();
    private ClientData clientData;

    [Serializable]
    public class Client
    {
        public string label;
        public int points;
        public bool isManager;
        public int id; // Include client ID
    }

    [Serializable]
    public class ClientData
    {
        public List<Client> clients;
        public Dictionary<string, ClientDetail> data;
        public string label;
    }

    [Serializable]
    public class ClientDetail
    {
        public string address;
        public string name;
        public int points;
    }

    private void Start()
    {
        FetchClients();
    }

    private void FetchClients()
    {
        StartCoroutine(GetClients());
    }

    private IEnumerator GetClients()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error fetching client data: " + webRequest.error);
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                ProcessClients(jsonResponse);
            }
        }
    }

    private void ProcessClients(string json)
    {
        Debug.Log("Received JSON response:\n" + json);

        clientData = JsonUtility.FromJson<ClientData>(json);

        if (clientData != null && clientData.clients != null)
        {
            Debug.Log("Number of clients: " + clientData.clients.Count);
            allClients = clientData.clients;
            FilterClients(filterDropdown.value);
        }
        else
        {
            clientListText.text = "No clients found in the data.";
        }
    }

    private void FilterClients(int filterIndex)
    {
        string clientInfo = "List of Clients:\n";
        foreach (Client client in allClients)
        {
            if (filterIndex == 0 ||
                (filterIndex == 1 && client.isManager) ||
                (filterIndex == 2 && !client.isManager))
            {
                clientInfo += FormatClientInfo(client);
            }
        }

        clientListText.text = clientInfo;
    }

    private string FormatClientInfo(Client client)
    {
        string detailedInfo = "";

        // Check if the client id exists in the data object
        if (clientData != null && clientData.data != null && clientData.data.ContainsKey(client.id.ToString()))
        {
            ClientDetail clientDetail = clientData.data[client.id.ToString()];
            detailedInfo = $"\nAddress: {clientDetail.address}\nName: {clientDetail.name}\nPoints: {clientDetail.points}";
        }

        return $"Client Label: {client.label}\nClient Points: {client.points}{detailedInfo}\n" +
               $"<color=blue><u><b>Click to View Details</b></u></color>\n";
    }
}

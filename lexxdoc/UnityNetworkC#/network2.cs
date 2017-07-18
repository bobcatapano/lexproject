using UnityEngine;
using System.Collections;
using SocketIO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class network2 : MonoBehaviour
{
    private SocketIOComponent socket;
    private string message;
    private GameObject player;
    private GameObject mycamera;

    private GameObject mDefaultModel;
    private GameObject mExtTrackedModel;
    private GameObject mNextTrackedModel;
    private GameObject mActiveModel = null;

    int counter = 1;
   
    private static readonly string POSTWishlistGetURL = "https://hooks.slack.com/services/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";

    public void Start()
    {
        Debug.Log("connecting");

        player = GameObject.FindGameObjectWithTag("Player");
        mycamera = GameObject.FindGameObjectWithTag("MainCamera");

        mDefaultModel = this.transform.Find("skeleton`").gameObject;
        mExtTrackedModel = this.transform.Find("bar4").gameObject;
        mNextTrackedModel = this.transform.Find("060317_pose03Muscles01").gameObject;

        mDefaultModel.SetActive(true);

        socket = GetComponent<SocketIOComponent>();

        socket.On("open", OnConnected);
        socket.On("new message", OnNewMessage);

        socket.Connect();     
    }

    public class ListRequest
    {
        public string text;
    }

    public WWW Post(string message)
    {
        WWWForm form = new WWWForm();

        // Create the parameter object for the request
        var request = new ListRequest { text = @message };

        // Convert to JSON (and to bytes)
        string jsonData = JsonUtility.ToJson(request);
        byte[] postData = System.Text.Encoding.ASCII.GetBytes(jsonData);

        Dictionary<string, string> postHeader = form.headers;
        if (postHeader.ContainsKey("Content-Type"))
            postHeader["Content-Type"] = "application/json";
        else
            postHeader.Add("Content-Type", "application/json");
        WWW www = new WWW(POSTWishlistGetURL, postData, postHeader);
        StartCoroutine(WaitForRequest(www));
        return www;
    }

    IEnumerator WaitForRequest(WWW data)
    {
        yield return data; // Wait until the download is done

        if (data.error != null)
        {
            //"There was an error sending request: " + data.error);
        }
        else
        {
            //WWW Request: " + data.text);
        }
    }

    public void OnConnected(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);

        var j = new JSONObject(JSONObject.Type.STRING);
        j.str = "VR";
        socket.Emit("add user", j);
    }

    private string ActiveModel()
    {
        if (mDefaultModel.activeSelf)
        {
            return "Dinosaur Allosaur Skeleton";
        }
        
        if (mExtTrackedModel.activeSelf)
        {
            return "Dinosaur Allosaur Skeleton and Muscle";
        }

        return "None";
    }

    public void OnNewMessage(SocketIOEvent e)
    {
        Debug.Log("[SocketIO] New Message received: " + e.name + " " + e.data);

        var username = e.data["username"].str;

        Debug.Log(username);

        if (username == "lex")
        {
            message = e.data["message"].str.ToLower();

            Debug.Log("Message:" + message);

            if (message == null) { return; }

           // string[] commands_array = message.Split(" "[0]);

            string[] commands_array = message.Split(":"[0]);

            if (commands_array[0] == "command")
            {
                var commandtype = commands_array[1];

                switch(commandtype)
                {
                    case "show":
                        if (commands_array[2] == "skeleton")
                        {
                            ShowDinosaur("skeleton");
                        }
                        else if (commands_array[2] == "muscles")
                        {
                            ShowDinosaur("skeleton_muscles");
                        }
                        break;
                         
                    case "tellmeaboutmodel":
                        string theMessage = "";
                                                
                        if (commands_array[2] == "dinosaur")
                        {
                            string TheActiveModel = ActiveModel();
                            
                            if (TheActiveModel == "Dinosaur Allosaur Skeleton")
                            {
                                theMessage = "This is the Allosaurs Skeleton. The Allosaur lived during the Jurassic period, between 155 to 150 million years ago";
                            }
                            else if (TheActiveModel == "Dinosaur Allosaur Skeleton and Muscle")
                            {
                                theMessage = "This is the Allosaurs Musclur system.";
                            }
                    
                        }
                        Post(theMessage);

                        break;
                    case "restarttheapp":
                         string[] scenePaths;
                         AssetBundle myLoadedAssetBundle;
                        myLoadedAssetBundle = AssetBundle.LoadFromFile("Assets/AssetBundles/scenes");

                        scenePaths = myLoadedAssetBundle.GetAllScenePaths();

                        // Just hard coding - restart AR the same as VR for now. 
                        if (commands_array[2].Contains("ar") || commands_array[2].Contains("vr"))
                        {
                            SceneManager.LoadScene(scenePaths[0], LoadSceneMode.Single);
                        }
                        break;
                    case "rotate":
                        if (commands_array[2] == "dinosaur")
                        {
                            float thenewfloat = 90f;

                            string degrees = commands_array[3];
                            string direction = commands_array[4];

                            //string[] newcommands_array = degrees.Split(" "[0]);

                            //degrees = newcommands_array[0];

                            float.TryParse(degrees, out thenewfloat);

                            string TheActiveModel = ActiveModel();

                            // Cooridantes were changed:
                            // Left is now down
                            // Right is no up
                            if (TheActiveModel == "Dinosaur Allosaur Skeleton")
                            {
                                if (direction.ToLower() == "left")
                                mDefaultModel.transform.Rotate(Vector3.down, 90.0f);
                                else if (direction.ToLower() == "right")
                                mDefaultModel.transform.Rotate(Vector3.up, thenewfloat);
                            }
                           else if (TheActiveModel == "Dinosaur Muscle")
                           {
                            if (direction.ToLower() == "left")
                                mExtTrackedModel.transform.Rotate(Vector3.down, thenewfloat);
                            else if (direction.ToLower() == "right")
                                mExtTrackedModel.transform.Rotate(Vector3.up, thenewfloat);
                           }
                        }
                        break;
                    case "startanimation":
                        string TheActiveModel2 = ActiveModel();

                        if (commands_array[2] == "dinosaur")
                        {
                            if (TheActiveModel2 == "Dinosaur Allosaur Skeleton")
                                mDefaultModel.GetComponent<Animator>().Play("Take 001");
                            else if (TheActiveModel2 == "Dinosaur Muscle")
                                mExtTrackedModel.GetComponent<Animator>().Play("Take 001");
                        }

                        break;

                }
            }

        }
    }

    private void ShowDinosaur(string model)
    {
        switch (model)
        {
            case "skeleton":
                mExtTrackedModel.SetActive(false);
                mDefaultModel.SetActive(true);
                mNextTrackedModel.SetActive(false);
                break;
            case "skeleton_muscles":
                mExtTrackedModel.SetActive(true);
                mDefaultModel.SetActive(false);
                mNextTrackedModel.SetActive(false);
                break;
            case "muscles":
                // TO DO: Just show the muscles    
                break;
            case "allosaurs":
                mExtTrackedModel.SetActive(false);
                mDefaultModel.SetActive(true);
                mNextTrackedModel.SetActive(false);
                break;
            case "barosaurus":
                mExtTrackedModel.SetActive(false);
                mDefaultModel.SetActive(false);
                mNextTrackedModel.SetActive(true);
                break;
        }
    }

    // This was used to explore, how to creete new objects.. To be used later..
    private void CreateObject(string color, string shape)
    {
        string name = "NewObject_" + counter;
        counter += 1;
        GameObject NewObject = new GameObject(name);

        switch (shape)
        {
            case "cube":
                NewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case "sphere":
                NewObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            case "cylinder":
                NewObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                break;
            case "capsule":
                NewObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                break;
        }

        NewObject.transform.position = new Vector3(0, 5, 0);
        NewObject.AddComponent<Rigidbody>();

        switch (color)
        {
            case "red":
                NewObject.GetComponent<Renderer>().material.color = Color.red;
                break;
            case "yellow":
                NewObject.GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case "green":
                NewObject.GetComponent<Renderer>().material.color = Color.green;
                break;
            case "blue":
                NewObject.GetComponent<Renderer>().material.color = Color.blue;
                break;
            case "black":
                NewObject.GetComponent<Renderer>().material.color = Color.black;
                break;
            case "white":
                NewObject.GetComponent<Renderer>().material.color = Color.white;
                break;
        }
    }
}

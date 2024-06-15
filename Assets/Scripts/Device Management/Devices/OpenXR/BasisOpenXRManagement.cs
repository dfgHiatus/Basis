using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
[System.Serializable]
public class BasisOpenXRManagement
{
    /// <summary>
    /// pulled in from InputDevices.GetDevices
    /// </summary>
    public List<InputDevice> inputDevices = new List<InputDevice>();
    /// <summary>
    /// generated at runtime
    /// </summary>
    public List<BasisOpenXRInput> TrackedOpenXRInputDevices = new List<BasisOpenXRInput>();
    /// <summary>
    /// keeps track of generated IDs and match InputDevice
    /// </summary>
    public Dictionary<string, InputDevice> TypicalDevices = new Dictionary<string, InputDevice>();
    public void StartXRSDK()
    {
        InputDevices.deviceConnected += OnDeviceConnected;
        InputDevices.deviceDisconnected += OnDeviceDisconnected;
        UpdateDeviceList();
    }
    public void StopXR()
    {
        if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
        {
            if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            }
        }
        foreach (BasisOpenXRInput BasisOpenVRInput in TrackedOpenXRInputDevices)
        {
            if (BasisOpenVRInput != null)
            {
                Object.Destroy(BasisOpenVRInput.gameObject);
            }
        }
        InputDevices.deviceConnected -= OnDeviceConnected;
        InputDevices.deviceDisconnected -= OnDeviceDisconnected;
    }

    private void OnDeviceConnected(UnityEngine.XR.InputDevice device)
    {
        UpdateDeviceList();
    }

    private void OnDeviceDisconnected(UnityEngine.XR.InputDevice device)
    {
        UpdateDeviceList();
    }
    private void UpdateDeviceList()
    {
        InputDevices.GetDevices(inputDevices);
        foreach (UnityEngine.XR.InputDevice device in inputDevices)
        {
            if (device.characteristics.HasFlag(InputDeviceCharacteristics.TrackingReference) == true)
            {
                continue;
            }

            if (device != null)
            {
                string ID = GenerateID(device);
                if (TypicalDevices.ContainsKey(ID) == false)
                {
                    CreatePhysicalTrackedDevice(device, ID, device.name);
                    TypicalDevices.Add(ID, device);
                }
            }
        }
        foreach (var deviceData in TypicalDevices)
        {
            if (deviceData.Value == null)
            {
                string ID = deviceData.Key;
                DestroyPhysicalTrackedDevice(ID);
            }
        }
    }
    public string GenerateID(InputDevice device)
    {
        string ID = device.name + "|" + device.serialNumber + "|" + device.manufacturer + "|" + (int)device.characteristics;
        return ID;
    }
    public void CreatePhysicalTrackedDevice(InputDevice device, string UniqueID, string UnUniqueID)
    {
        GameObject gameObject = new GameObject(UniqueID);
        gameObject.transform.parent = BasisLocalPlayer.Instance.LocalBoneDriver.transform;
        BasisOpenXRInput BasisXRInput = gameObject.AddComponent<BasisOpenXRInput>();
        BasisXRInput.Initialize(device, UniqueID, UnUniqueID);
        TrackedOpenXRInputDevices.Add(BasisXRInput);
        BasisDeviceManagement.Instance.AllInputDevices.Add(BasisXRInput);
    }
    /// <summary>
    /// this wont well with fullbody, revist later
    /// </summary>
    /// <param name="ID"></param>
    public void DestroyPhysicalTrackedDevice(string ID)
    {
        DestroyInputDevice(ID);
        DestroyXRInput(ID);
    }
    public void DestroyInputDevice(string ID)
    {
        foreach (var device in TypicalDevices)
        {
            if (device.Key == ID)
            {
                TypicalDevices.Remove(ID);
                break;
            }
        }
    }
    public void DestroyXRInput(string ID)
    {
        foreach (var device in TrackedOpenXRInputDevices)
        {
            if (device.UniqueID == ID)
            {
                TrackedOpenXRInputDevices.Remove(device);
                Object.Destroy(device.gameObject);
                break;
            }
        }
        List<BasisInput> Duplicate = new List<BasisInput>();
        Duplicate.AddRange(BasisDeviceManagement.Instance.AllInputDevices);
        foreach (var device in Duplicate)
        {
            if (device.UniqueID == ID)
            {
                BasisDeviceManagement.Instance.AllInputDevices.Remove(device);
            }
        }
    }
}
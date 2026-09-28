using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlazFxDemo : MonoBehaviour {

    public GameObject HelpPanel = null;
    public Camera Camera = null;
    public GameObject Caustic = null;
    public GameObject DiveMask = null;
    public GameObject UnderwaterFX = null;

    private bool _help = false;
    private bool _blur = false;
    private bool _sunshaft = false;
    private bool _fisheye = false;
    private bool _vortex = false;
    private bool _wave = false;

    private bool _caustic = false;
    private bool _mask = false;
    private bool _ufx = false;
    private bool _sky = false;

    // Update is called once per frame
    void Start()
    {
        _blur = Camera.gameObject.GetComponent<GlazUnderwaterFX.UwBlur>().enabled;
        _sunshaft = Camera.gameObject.GetComponent<GlazUnderwaterFX.UwSunShafts>().enabled;
        _fisheye = Camera.gameObject.GetComponent<GlazUnderwaterFX.UwFisheye>().enabled;
        _vortex = Camera.gameObject.GetComponent<GlazUnderwaterFX.UwVortexAnimated>().enabled;
        _wave = Camera.gameObject.GetComponent<GlazUnderwaterFX.UwWaveAnimated>().enabled;

        _help = HelpPanel.activeInHierarchy;
        _caustic = Caustic.activeInHierarchy;
        _mask = DiveMask.activeInHierarchy;
        _ufx = UnderwaterFX.activeInHierarchy;
        if (Camera.clearFlags == CameraClearFlags.Skybox)
            _sky = true;
        else
            _sky = false;
    }

    void Update () {

        if (Input.GetKeyDown(KeyCode.H))
        {
            _help = !_help;
            HelpPanel.SetActive(_help);
        }


        if (Input.GetKeyDown(KeyCode.X))
        {
            _wave = !_wave;
            Camera.gameObject.GetComponent<GlazUnderwaterFX.UwWaveAnimated>().enabled = _wave;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            _caustic = !_caustic;
            Caustic.SetActive(_caustic);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            _vortex = !_vortex;
            Camera.gameObject.GetComponent<GlazUnderwaterFX.UwVortexAnimated>().enabled = _vortex;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            _blur = !_blur;
            Camera.gameObject.GetComponent<GlazUnderwaterFX.UwBlur>().enabled = _blur;
            Camera.gameObject.GetComponent<GlazUnderwaterFX.UwEdgeDetection>().enabled = _blur;
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            _sunshaft = !_sunshaft;
            Camera.gameObject.GetComponent<GlazUnderwaterFX.UwSunShafts>().enabled = _sunshaft;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            _fisheye = !_fisheye;            
            Camera.gameObject.GetComponent<GlazUnderwaterFX.UwFisheye>().enabled = _fisheye;
        }


        if (Input.GetKeyDown(KeyCode.J))
        {
            _mask = !_mask;
            DiveMask.SetActive(_mask);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            _ufx = !_ufx;
            UnderwaterFX.SetActive(_ufx);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            _sky = !_sky;
            UnderwaterFX.SetActive(_ufx);

            if (_sky)
                Camera.clearFlags = CameraClearFlags.Skybox;
            else
                Camera.clearFlags = CameraClearFlags.SolidColor;
        }
    }

}

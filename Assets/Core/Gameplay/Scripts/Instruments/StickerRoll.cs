using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StickerRoll : MonoBehaviour
{
    public static event Func<float[]> OnStickerEnabled;
    public static event Action OnStickerFinished;
    
    [Header("Material Settings")]
    [SerializeField] private Material stickerMaterial;
    [SerializeField] private Shader stickerRollShader;
    [SerializeField] private Shader stickerPaintShader;
    
    [Space]
    [SerializeField] private float stickerBounds;
    [SerializeField] private Vector2 stickerEnterPoint;
    [SerializeField] private Vector2 stickerStartingPoint;
    [SerializeField] private float maxHorizontalOffset=0.2f;

    private Vector3 currentPos;
    private Vector2 stickerCurrentPoint;
    private float progress;
    private float minVPoint, maxVPoint;

    private void OnEnable()
    {
        stickerMaterial.shader = stickerRollShader;

        float[] uvBounds = OnStickerEnabled?.Invoke()??new []{0f,0f,0f,0f};
        minVPoint = uvBounds[0];
        maxVPoint = uvBounds[1];
        maxVPoint -= 0.1f;//sticker radius
        stickerStartingPoint.y = minVPoint;
        stickerCurrentPoint = stickerStartingPoint;
        
        stickerMaterial.SetVector("_StickerEnterPoint",stickerEnterPoint);
        stickerMaterial.SetVector("_StickerPointer",stickerStartingPoint );
        
        Shader.SetGlobalVector("_MinUVBounds",new Vector2(uvBounds[2],uvBounds[0]));
        Shader.SetGlobalVector("_MaxUVBounds",new Vector2(uvBounds[3],uvBounds[1]));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentPos = GetTouchProjectionPosition();
        }
        
        if (Input.GetMouseButton(0) && !IsTouchUI())
        {
            Vector3 desiredPosition = GetTouchProjectionPosition();
            Vector2 targetVelocity = (desiredPosition-currentPos)/stickerBounds;
            currentPos = desiredPosition;
            stickerCurrentPoint +=targetVelocity;

            stickerCurrentPoint.x = Mathf.Clamp(stickerCurrentPoint.x,stickerEnterPoint.x-maxHorizontalOffset,stickerEnterPoint.x+maxHorizontalOffset);
            stickerCurrentPoint.y = Mathf.Clamp(stickerCurrentPoint.y,minVPoint,maxVPoint);
            stickerMaterial.SetVector("_StickerPointer", stickerCurrentPoint);

            progress = stickerCurrentPoint.y /maxVPoint;
            if (progress >= 1f)
            {
                enabled = false; 
                StartCoroutine(FinishStickerRoutine());
            }
        }
    }

    private IEnumerator FinishStickerRoutine()
    {
        Vector3 targetPointer = stickerCurrentPoint;
        Vector3 staringPointer = stickerCurrentPoint;
        targetPointer.y = 0.999f;
        float t = 0;
        float speed = 1/(Mathf.Abs(stickerCurrentPoint.x - stickerEnterPoint.x) /(2*maxHorizontalOffset));
        
        while (t<1)
        {
            t += speed * Time.deltaTime;
            stickerCurrentPoint = Vector2.Lerp(staringPointer,targetPointer,t);
            stickerMaterial.SetVector("_StickerPointer", stickerCurrentPoint);
            yield return null;
        }
        
        OnStickerFinished?.Invoke();
        stickerMaterial.shader = stickerPaintShader;
    }
    
    private Vector3 GetTouchProjectionPosition()
    {
        Vector3 pos = Input.mousePosition;
        pos.z = transform.position.z - Camera.main.transform.position.z;
        pos = Camera.main.ScreenToWorldPoint(pos);

        return pos;
    }
    
    private bool IsTouchUI()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current) {position = Input.mousePosition};

        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);
        return raycastResults.Count >0;
    }

    public void Unroll(Action onComplete)
    {
        stickerMaterial.shader = stickerRollShader;
        stickerMaterial.SetVector("_StickerEnterPoint",stickerEnterPoint);
        stickerMaterial.SetVector("_StickerPointer",stickerCurrentPoint );
        StartCoroutine(UnrollRoutine(1,onComplete));
    }

    private IEnumerator UnrollRoutine(float duration, Action onComplete)
    {
        Vector3 targetPointer = stickerCurrentPoint;
        Vector3 staringPointer = stickerCurrentPoint;
        targetPointer.y = minVPoint;
        targetPointer.x = stickerEnterPoint.x - maxHorizontalOffset;
        float t = 0;
        float speed = 1/((stickerCurrentPoint.y-minVPoint)/(1-minVPoint));

        while (t<1)
        {
            t += speed * Time.deltaTime;
            stickerCurrentPoint = Vector2.Lerp(staringPointer,targetPointer,t);
            stickerMaterial.SetVector("_StickerPointer", stickerCurrentPoint);
            yield return null;
        }
        
        onComplete?.Invoke();
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class VagabundoController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 4.5f;
    public float velocidadCorrer = 8f;
    public float aceleracionAire = 0.85f;
    public float aceleracionHorizontal = 80f;
    public float distanciaDeteccionPared = 0.05f;

    [Header("Salto")]
    public float fuerzaSalto = 12f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;
    public float jumpCutMultiplier = 0.5f;

    [Header("Suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Ajustes")]
    public bool mostrarDebug = false;
    public bool forzarSinFriccion = true;

    [Header("Vacilar")]
    public float duracionVacile = 0.45f;
    public float cooldownVacile = 0.3f;

    [Header("Daño")]
    public float empujeDanioX = 5.5f;
    public float empujeDanioY = 3.5f;
    public float bloqueoControlesDanio = 0.18f;
    public float parpadeoDanioTiempo = 0.22f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Collider2D cuerpoCollider;
    private PhysicsMaterial2D materialSinFriccion;

    private float movH;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private float vacileDisponibleEn;

    private bool saltoSoltado;
    private bool controlesBloqueados;
    private bool estaMuerto;
    private bool estaCorriendo;
    private bool corriaAlSaltar;
    private bool estaVacilando;
    private bool enDanio;
    private bool enSuelo;
    private bool enSueloPrevio;

    private Vector3 spawnInicial;
    private Vector3 spawnPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        cuerpoCollider = GetComponent<Collider2D>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        AplicarMaterialSinFriccionSiCorresponde();

        spawnInicial = transform.position;
        spawnPoint = transform.position;
    }

    private void Update()
    {
        if (InputBloqueado())
        {
            movH = 0f;
            return;
        }

        movH = LeerHorizontal();
        estaCorriendo = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Mathf.Abs(movH) > 0.01f;

        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        if (Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.Space))
        {
            saltoSoltado = true;
        }

        ActualizarDireccionVisual();
        ActualizarAnimator();

        if (Input.GetKeyDown(KeyCode.E) && PuedeVacilar())
        {
            StartCoroutine(HacerVacile());
        }

        jumpBufferCounter -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        ActualizarSuelo();
        AplicarMovimientoHorizontal();
        IntentarSalto();
        AplicarJumpCut();
        CorregirPegadoEnPared();

        saltoSoltado = false;
    }

    private void AplicarMaterialSinFriccionSiCorresponde()
    {
        if (!forzarSinFriccion || cuerpoCollider == null) return;

        materialSinFriccion = new PhysicsMaterial2D("PlayerNoFriction")
        {
            friction = 0f,
            bounciness = 0f
        };
        cuerpoCollider.sharedMaterial = materialSinFriccion;
    }

    private bool InputBloqueado()
    {
        return controlesBloqueados || enDanio;
    }

    private void ActualizarDireccionVisual()
    {
        if (movH > 0.01f)
        {
            transform.localScale = Vector3.one;
        }
        else if (movH < -0.01f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    private void ActualizarAnimator()
    {
        if (anim == null) return;

        float velAnim = Mathf.Max(Mathf.Abs(movH), Mathf.Abs(rb.linearVelocity.x));
        SetFloatIfExists("Velocidad", velAnim);
        SetFloatIfExists("VelY", rb.linearVelocity.y);
        SetBoolIfExists("EnSuelo", enSuelo);
        SetBoolIfExists("Corriendo", estaCorriendo);
        SetBoolIfExists("Muerto", estaMuerto);
        SetBoolIfExists("Muerte", estaMuerto);
    }

    private void ActualizarSuelo()
    {
        enSuelo = DetectarSuelo();
        coyoteCounter = enSuelo ? coyoteTime : coyoteCounter - Time.fixedDeltaTime;

        if (enSuelo != enSueloPrevio)
        {
            corriaAlSaltar = estaCorriendo;
            enSueloPrevio = enSuelo;
        }
    }

    private bool DetectarSuelo()
    {
        if (groundCheck == null)
        {
            return Mathf.Abs(rb.linearVelocity.y) < 0.05f;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] != null && hits[i].attachedRigidbody != rb)
            {
                return true;
            }
        }

        return false;
    }

    private void AplicarMovimientoHorizontal()
    {
        bool usarModoCorrer = enSuelo ? estaCorriendo : corriaAlSaltar;
        float velocidadObjetivo = movH * (usarModoCorrer ? velocidadCorrer : velocidadCaminar);
        float aceleracion = enSuelo ? aceleracionHorizontal : Mathf.Max(1f, aceleracionHorizontal * aceleracionAire);
        float maxDelta = aceleracion * Time.fixedDeltaTime;
        float velX = Mathf.MoveTowards(rb.linearVelocity.x, velocidadObjetivo, maxDelta);
        rb.linearVelocity = new Vector2(velX, rb.linearVelocity.y);
    }

    private void IntentarSalto()
    {
        if (jumpBufferCounter <= 0f || coyoteCounter <= 0f) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;
        corriaAlSaltar = estaCorriendo;
        enSuelo = false;
        SetTriggerIfExists("Jump");

        GameManager gm = GameManager.Instance;
        if (gm != null) gm.PlayJumpSfx();
    }

    private void AplicarJumpCut()
    {
        if (saltoSoltado && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    private void CorregirPegadoEnPared()
    {
        if (enSuelo) return;
        if (Mathf.Abs(rb.linearVelocity.y) < 0.05f) return;
        if (!EstaTocandoPared(out float direccionPared)) return;

        bool empujaContraPared = (direccionPared > 0f && movH > 0.01f) || (direccionPared < 0f && movH < -0.01f);
        if (!empujaContraPared) return;

        float velX = rb.linearVelocity.x;
        if ((direccionPared > 0f && velX > 0f) || (direccionPared < 0f && velX < 0f))
        {
            velX = 0f;
            rb.linearVelocity = new Vector2(velX, rb.linearVelocity.y);
        }
    }

    private bool EstaTocandoPared(out float direccionPared)
    {
        direccionPared = 0f;
        if (cuerpoCollider == null) return false;

        Bounds b = cuerpoCollider.bounds;
        float anchoDeteccion = Mathf.Max(distanciaDeteccionPared, 0.01f);
        Vector2 areaDeteccion = new Vector2(anchoDeteccion, b.size.y * 0.55f);
        float yDeteccion = b.center.y + (b.extents.y * 0.15f);

        Vector2 posDerecha = new Vector2(b.max.x + anchoDeteccion * 0.5f, yDeteccion);
        Collider2D hitDerecha = Physics2D.OverlapBox(posDerecha, areaDeteccion, 0f, groundLayer);
        if (EsColliderValidoParaPared(hitDerecha))
        {
            direccionPared = 1f;
            return true;
        }

        Vector2 posIzquierda = new Vector2(b.min.x - anchoDeteccion * 0.5f, yDeteccion);
        Collider2D hitIzquierda = Physics2D.OverlapBox(posIzquierda, areaDeteccion, 0f, groundLayer);
        if (EsColliderValidoParaPared(hitIzquierda))
        {
            direccionPared = -1f;
            return true;
        }

        return false;
    }

    private bool EsColliderValidoParaPared(Collider2D hit)
    {
        return hit != null && hit.attachedRigidbody != rb;
    }

    public void SetSpawnPoint(Vector3 newSpawn)
    {
        spawnPoint = newSpawn;
    }

    public void Respawn()
    {
        estaMuerto = false;
        controlesBloqueados = false;
        enDanio = false;

        if (anim != null)
        {
            SetBoolIfExists("Muerto", false);
            SetBoolIfExists("Muerte", false);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        rb.linearVelocity = Vector2.zero;
        transform.position = spawnPoint;
    }

    public void ForzarSpawnInicial(Vector3 newSpawn)
    {
        spawnInicial = newSpawn;
        spawnPoint = newSpawn;
        transform.position = spawnPoint;
        rb.linearVelocity = Vector2.zero;
    }

    public void RespawnAlInicio()
    {
        spawnPoint = spawnInicial;
        Respawn();
    }

    public void ReproducirDanio()
    {
        if (anim != null && !estaMuerto)
        {
            SetTriggerIfExists("Hurt");
        }
    }

    public void AplicarEmpujeDanio(Vector2 fuenteDanio)
    {
        if (estaMuerto) return;

        ReproducirDanio();
        StopCoroutine(nameof(CorutinaDanioVisual));
        StartCoroutine(CorutinaDanioVisual());

        float dir = Mathf.Sign(((Vector2)transform.position - fuenteDanio).x);
        if (Mathf.Approximately(dir, 0f))
        {
            dir = transform.localScale.x >= 0f ? 1f : -1f;
        }

        rb.linearVelocity = new Vector2(dir * empujeDanioX, empujeDanioY);
        StartCoroutine(CorutinaBloqueoDanio());
    }

    public void Morir()
    {
        estaMuerto = true;
        controlesBloqueados = true;
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
        {
            SetBoolIfExists("Muerto", true);
            SetBoolIfExists("Muerte", true);
            SetTriggerIfExists("Dead");
        }
    }

    private bool PuedeVacilar()
    {
        return enSuelo && !estaMuerto && !estaVacilando && Time.time >= vacileDisponibleEn && anim != null;
    }

    private System.Collections.IEnumerator HacerVacile()
    {
        estaVacilando = true;
        controlesBloqueados = true;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        SetTriggerIfExists("Vacilar");

        yield return new WaitForSeconds(duracionVacile);

        if (!estaMuerto)
        {
            controlesBloqueados = false;
        }

        estaVacilando = false;
        vacileDisponibleEn = Time.time + cooldownVacile;
    }

    private System.Collections.IEnumerator CorutinaBloqueoDanio()
    {
        enDanio = true;
        controlesBloqueados = true;
        yield return new WaitForSeconds(bloqueoControlesDanio);
        enDanio = false;

        if (!estaMuerto && !estaVacilando)
        {
            controlesBloqueados = false;
        }
    }

    private System.Collections.IEnumerator CorutinaDanioVisual()
    {
        if (spriteRenderer == null) yield break;

        Color original = Color.white;
        Color golpe = new Color(1f, 0.45f, 0.45f, 1f);
        float t = 0f;

        while (t < parpadeoDanioTiempo)
        {
            spriteRenderer.color = golpe;
            yield return new WaitForSeconds(0.05f);
            spriteRenderer.color = original;
            yield return new WaitForSeconds(0.05f);
            t += 0.1f;
        }

        spriteRenderer.color = original;
    }

    private static float LeerHorizontal()
    {
        bool izquierda = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool derecha = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        if (izquierda == derecha) return 0f;
        return izquierda ? -1f : 1f;
    }

    private void SetBoolIfExists(string param, bool value)
    {
        if (HasParameter(param, AnimatorControllerParameterType.Bool))
        {
            anim.SetBool(param, value);
        }
    }

    private void SetFloatIfExists(string param, float value)
    {
        if (HasParameter(param, AnimatorControllerParameterType.Float))
        {
            anim.SetFloat(param, value);
        }
    }

    private void SetTriggerIfExists(string param)
    {
        if (HasParameter(param, AnimatorControllerParameterType.Trigger))
        {
            anim.SetTrigger(param);
        }
    }

    private bool HasParameter(string param, AnimatorControllerParameterType type)
    {
        if (anim == null) return false;

        AnimatorControllerParameter[] parameters = anim.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].name == param && parameters[i].type == type)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!mostrarDebug) return;

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Collider2D c = GetComponent<Collider2D>();
        if (c == null) return;

        Bounds b = c.bounds;
        float anchoDeteccion = Mathf.Max(distanciaDeteccionPared, 0.01f);
        Vector2 areaDeteccion = new Vector2(anchoDeteccion, b.size.y * 0.55f);
        float yDeteccion = b.center.y + (b.extents.y * 0.15f);
        Vector2 posDerecha = new Vector2(b.max.x + anchoDeteccion * 0.5f, yDeteccion);
        Vector2 posIzquierda = new Vector2(b.min.x - anchoDeteccion * 0.5f, yDeteccion);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(posDerecha, areaDeteccion);
        Gizmos.DrawWireCube(posIzquierda, areaDeteccion);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MedievalJourney
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class OpeningSequenceUI : MonoBehaviour
    {
        private static readonly string[] KickerPages =
        {
            "PROLOGO  •  A ALDEIA",
            "PROLOGO  •  A INVASAO",
            "SUA MISSAO",
            "PREPARE SUA ESPADA"
        };

        private static readonly string[] TitlePages =
        {
            "UMA SOMBRA CAIU SOBRE A ALDEIA...",
            "O MEDO TOMOU OS CAMINHOS",
            "A ALDEIA PRECISA DE VOCE",
            "CONTROLES DO CAVALEIRO"
        };

        private static readonly string[] StoryPages =
        {
            "Ao cair da noite, passos estranhos romperam a paz.\n" +
            "Tochas surgiram entre as casas e os portoes foram tomados.",
            "Bandidos invadiram estas terras e ameacam os moradores.\n" +
            "A frente deles esta um lider cruel, escondido alem da aldeia.",
            "Atravesse os caminhos, derrote os bandidos e alcance a arena final.\n" +
            "Somente a queda do lider podera libertar o povo.",
            "MOVER  •  A/D ou setas     PULAR  •  Espaco/W\n" +
            "ATACAR  •  J/clique esquerdo     ESCUDO  •  K/clique direito"
        };

        [SerializeField] private CanvasGroup screenGroup;
        [SerializeField] private CanvasGroup contentGroup;
        [SerializeField] private Text kickerText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text storyText;
        [SerializeField] private Text pageText;
        [SerializeField] private Text nextButtonText;
        [SerializeField] private GameObject controlsAccent;
        [SerializeField] private GameManager gameManager;

        private Coroutine transitionRoutine;
        private int pageIndex;
        private float inputUnlockTime;
        private bool finishing;

        public int CurrentPage => pageIndex;
        public int PageCount => TitlePages.Length;

        public void Configure(CanvasGroup screen, CanvasGroup content, Text kicker, Text title,
            Text story, Text page, Text nextLabel, GameObject controls, GameManager manager)
        {
            screenGroup = screen;
            contentGroup = content;
            kickerText = kicker;
            titleText = title;
            storyText = story;
            pageText = page;
            nextButtonText = nextLabel;
            controlsAccent = controls;
            gameManager = manager;
        }

        public void Begin()
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
            }

            gameObject.SetActive(true);
            pageIndex = 0;
            finishing = false;
            inputUnlockTime = Time.unscaledTime + 0.35f;
            screenGroup.alpha = 0f;
            contentGroup.alpha = 1f;
            ApplyPage();
            transitionRoutine = StartCoroutine(FadeScreen(1f, 0.45f));
        }

        public void AdvancePage()
        {
            if (finishing || transitionRoutine != null || Time.unscaledTime < inputUnlockTime)
            {
                return;
            }

            if (pageIndex >= TitlePages.Length - 1)
            {
                Finish();
                return;
            }

            transitionRoutine = StartCoroutine(ChangePage());
        }

        public void Skip()
        {
            if (!finishing)
            {
                Finish();
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || Time.unscaledTime < inputUnlockTime)
            {
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                Skip();
            }
            else if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame ||
                     keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                AdvancePage();
            }
        }

        private IEnumerator ChangePage()
        {
            yield return Fade(contentGroup, 0f, 0.18f);
            pageIndex++;
            ApplyPage();
            yield return Fade(contentGroup, 1f, 0.24f);
            inputUnlockTime = Time.unscaledTime + 0.12f;
            transitionRoutine = null;
        }

        private void Finish()
        {
            finishing = true;
            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
            }

            transitionRoutine = StartCoroutine(FinishSequence());
        }

        private IEnumerator FinishSequence()
        {
            yield return Fade(screenGroup, 0f, 0.4f);
            transitionRoutine = null;
            finishing = false;
            gameManager?.CompleteIntroduction();
        }

        private IEnumerator FadeScreen(float target, float duration)
        {
            yield return Fade(screenGroup, target, duration);
            transitionRoutine = null;
        }

        private static IEnumerator Fade(CanvasGroup group, float target, float duration)
        {
            float start = group.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                group.alpha = Mathf.Lerp(start, target, t);
                yield return null;
            }

            group.alpha = target;
        }

        private void ApplyPage()
        {
            kickerText.text = KickerPages[pageIndex];
            titleText.text = TitlePages[pageIndex];
            storyText.text = StoryPages[pageIndex];
            pageText.text = $"{pageIndex + 1}  /  {TitlePages.Length}";
            nextButtonText.text = pageIndex == TitlePages.Length - 1 ? "COMECAR" : "CONTINUAR";
            if (controlsAccent != null)
            {
                controlsAccent.SetActive(pageIndex == TitlePages.Length - 1);
            }
        }
    }
}

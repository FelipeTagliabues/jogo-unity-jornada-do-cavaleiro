using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Quiz1SceneValidator
{
    [MenuItem("Projeto Medieval/Validar cena do Quiz 1")]
    public static void ValidateScene()
    {
        var errors = new List<string>();
        GameObject player = GameObject.Find("Cavaleiro");

        if (player == null)
        {
            errors.Add("o objeto Cavaleiro nao foi encontrado.");
        }
        else
        {
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body == null || body.bodyType != RigidbodyType2D.Dynamic || body.gravityScale <= 0f)
            {
                errors.Add("o Cavaleiro precisa de um Rigidbody2D dinamico com gravidade.");
            }

            if (player.GetComponent<Collider2D>() == null)
            {
                errors.Add("o Cavaleiro precisa de um Collider2D.");
            }

            if (player.GetComponent<PlayerMovement2D>() == null)
            {
                errors.Add("o script PlayerMovement2D nao esta associado ao Cavaleiro.");
            }

            Animator animator = player.GetComponent<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                errors.Add("o Animator do Cavaleiro nao possui controller.");
            }

            if (player.transform.Find("GroundSensor") == null)
            {
                errors.Add("o detector GroundSensor nao foi encontrado.");
            }
        }

        int groundLayer = LayerMask.NameToLayer("Ground");
        bool foundGroundCollider = false;
        if (groundLayer >= 0)
        {
            foreach (Collider2D sceneCollider in Object.FindObjectsByType<Collider2D>())
            {
                if (!sceneCollider.isTrigger && sceneCollider.gameObject.layer == groundLayer)
                {
                    foundGroundCollider = true;
                    break;
                }
            }
        }

        if (!foundGroundCollider)
        {
            errors.Add("nao existe terreno solido com Collider2D na camada Ground.");
        }

        if (errors.Count == 0)
        {
            Debug.Log("Quiz 1 validado: movimento, Rigidbody2D, Collider2D, GroundSensor e Animator estao configurados.");
            return;
        }

        foreach (string error in errors)
        {
            Debug.LogError($"Quiz 1: {error}");
        }
    }
}

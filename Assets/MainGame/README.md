# A Jornada do Cavaleiro

Jogo medieval 2D de plataforma e exploracao criado para a disciplina Desenvolvimento de Jogos
Digitais. A fase usa o tileset e os prefabs reais do pacote Cainos Village Props.

Os pacotes da Unity Asset Store usados pela cena nao sao redistribuidos no repositorio publico.
Antes de abrir o projeto clonado, siga as instrucoes em `THIRD_PARTY_ASSETS.md` na raiz.

## Executar

1. Abra o projeto na Unity `6000.6.0f1`.
2. Abra `Assets/Scenes/MainGame.unity`.
3. Pressione **Play** e use o botao **Iniciar**. Uma abertura narrativa curta apresenta a
   invasao, a missao e os controles antes de liberar o cavaleiro.

`MainGame` tambem e a primeira cena habilitada nas Build Settings. As cenas e implementacoes dos
Quizzes 1 e 2 permanecem preservadas.

## Controles

- `A` / `D` ou setas esquerda/direita: andar.
- `Espaco`, `W` ou seta para cima: pular.
- `J` ou clique esquerdo: atacar.
- `K` ou clique direito: levantar o escudo e bloquear ataques frontais.
- `Esc`: pausar ou continuar.

Durante a introducao, `Espaco`/`Enter` avancam os quadros. O botao **Pular**, `S` ou `Esc`
encerram a abertura imediatamente. A simulacao, o cronometro e os inimigos permanecem congelados
ate o inicio efetivo da fase; reiniciar depois de uma derrota ou vitoria entra direto no gameplay.

## Objetivo

Atravesse sete regioes conectadas: entrada, aldeia medieval, bosque, ponte, acampamento bandido,
ruinas e arena final. A fase possui 335 unidades de extensao, 16 inimigos, plataformas opcionais,
oito provisoes de vida e um lider mais resistente. Elimine o lider e alcance o estandarte dourado.

Macas recuperam 1 ponto de vida e frascos recuperam 2, sempre respeitando a vida maxima. Os itens
ficam em plataformas e rotas que recompensam exploracao. O HUD atualiza vida, pontos, inimigos e
tempo imediatamente apos a coleta.

## Ferramentas de Editor

- `Projeto Medieval > Construir jogo completo > MainGame`: regenera a cena de forma reproduzivel.
- `Projeto Medieval > Validar jogo completo > MainGame`: verifica os componentes, inimigos,
  Tilemap Cainos, colisores, itens de vida, interface, botoes, objetivo e Build Settings.
- `Projeto Medieval > Testar MainGame em Play Mode`: executa o smoke test de menu, introducao,
  pulo da abertura, fisica, camera, cura, pausa, derrota, reinicio direto, combate, pontuacao,
  chefe e vitoria.
- `Projeto Medieval > Gerar build Windows de entrega`: gera a pasta
  `Builds/AJornadaDoCavaleiro` quando a licenca local da Unity estiver disponivel.

O pacote nao possui audio; dano, ataques, estados e resultados usam animacao, cor e interface como
feedback visual.

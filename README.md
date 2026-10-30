# A Jornada do Cavaleiro

Jogo medieval 2D de plataforma e exploração desenvolvido em Unity. O cavaleiro atravessa uma
aldeia invadida, enfrenta bandidos, encontra provisões e derrota o líder inimigo na arena final.

O projeto preserva as cenas dos Quizzes 1 e 2 e adiciona uma fase completa com introdução
narrativa, sete regiões conectadas, combate, defesa, vida, pontuação, cronômetro, itens de cura,
condições de vitória e derrota e menus de jogo.

## Requisitos

- Unity `6000.6.0f1` (`Unity 6.6`), revisão `f7f8ed4d1e24`.
- Git para clonar o repositório.
- Conta gratuita da Unity Asset Store para obter quatro pacotes gratuitos usados pelo projeto.
- Acesso ao Pixabay e ao Mixkit para restaurar os cinco áudios gratuitos nos nomes documentados.

As dependências do Unity Package Manager estão registradas em `Packages/manifest.json` e
`Packages/packages-lock.json`.

## Assets necessários

Os arquivos-fonte dos pacotes abaixo não são redistribuídos neste repositório porque usam a
Standard Unity Asset Store EULA. Antes de abrir a cena principal, obtenha e importe os pacotes
pela Unity Asset Store/My Assets:

| Pacote | Versão usada | Product ID |
| --- | --- | --- |
| [Bandits - Pixel Art](https://assetstore.unity.com/packages/2d/characters/bandits-pixel-art-104130) | 2.2 | 104130 |
| [Hero Knight - Pixel Art](https://assetstore.unity.com/packages/2d/characters/hero-knight-pixel-art-165188) | 1.2 | 165188 |
| [Pixel Art Platformer - Village Props](https://assetstore.unity.com/packages/2d/environments/pixel-art-platformer-village-props-166114) | 2.3.2 | 166114 |
| [Fantasy Wooden GUI : Free](https://assetstore.unity.com/packages/2d/gui/fantasy-wooden-gui-free-103811) | 2.0 no projeto; 2.1 atual | 103811 |

Consulte [THIRD_PARTY_ASSETS.md](THIRD_PARTY_ASSETS.md) para os diretórios esperados, detalhes de
licença e observações de reprodução.

Os cinco arquivos de áudio também são mantidos fora do Git: as licenças permitem incorporá-los ao
jogo, mas restringem a redistribuição dos arquivos-fonte/isolados. Baixe as faixas e efeitos pelos
links registrados em `THIRD_PARTY_ASSETS.md`, coloque-os em `Assets/EfeitosSonoros/` com estes nomes
exatos e preserve os arquivos `.meta` versionados:

- `MusicaDeFundoMenu.mp3`
- `MusicaDeFundoDoJogoRodando.mp3`
- `AtaqueEspada.wav`
- `escudo.mp3`
- `SinoAoMorrer.mp3`

## Instalação e execução

1. Clone o repositório.
2. No Unity Hub, instale ou selecione a Unity `6000.6.0f1`.
3. Adicione o diretório clonado como projeto.
4. Importe os quatro pacotes da tabela acima, preservando os caminhos e arquivos `.meta` gerados
   pelos pacotes.
5. Restaure os cinco áudios nos nomes e caminhos indicados acima, sem substituir os `.meta` do Git.
6. Aguarde a importação e execute `Projeto Medieval > Validar jogo completo > MainGame`.
7. Abra `Assets/Scenes/MainGame.unity`, pressione **Play** e selecione **Iniciar**.

`MainGame` é a primeira cena habilitada em Build Settings. Se algum pacote tiver sido importado em
um caminho diferente, restaure os diretórios indicados em `THIRD_PARTY_ASSETS.md` e execute
`Projeto Medieval > Construir jogo completo > MainGame`.

## Controles

- `A` / `D` ou setas: movimentar.
- `Espaço`, `W` ou seta para cima: pular.
- `J` ou clique esquerdo: atacar.
- `K` ou clique direito: usar o escudo.
- `Esc`: pausar/continuar durante a fase.
- Na introdução, `Espaço`/`Enter` avançam e `S`, `Esc` ou **Pular** encerram a abertura.

## Objetivo e conteúdo

A fase percorre entrada da aldeia, praça, bosque, ponte, acampamento, ruínas e arena. Ela contém
16 inimigos, oito itens de cura, plataformas opcionais e um líder mais resistente. Derrote o líder
e alcance o estandarte dourado para concluir a missão.

## Validação no Editor

O menu `Projeto Medieval` oferece três rotinas principais:

- `Construir jogo completo > MainGame`: regenera a cena principal de forma determinística.
- `Validar jogo completo > MainGame`: verifica referências, terreno, inimigos, itens, UI, câmera e
  Build Settings.
- `Testar MainGame em Play Mode`: executa o smoke test de introdução, gameplay, física, cura,
  defesa, pausa, derrota, reinício, combate, chefe e vitória.

Na preparação desta versão, a compilação C# terminou sem erros, a validação estrutural, o smoke test
completo em Play Mode e um build Windows de desenvolvimento foram aprovados. O único aviso C#
restante (`CS0105`) vem de um script original do pacote Cainos e não impede a execução.

## Estrutura relevante

- `Assets/Scenes/MainGame.unity`: cena inicial do jogo completo.
- `Assets/MainGame/Runtime`: sistemas de gameplay e interface.
- `Assets/MainGame/Editor`: gerador, validador e smoke test.
- `Assets/Scenes/Quiz1_Movimentacao.unity`: cena preservada do Quiz 1/Quiz 2.
- `Packages`: dependências do Unity Package Manager.
- `ProjectSettings`: configuração necessária para reproduzir o projeto.

Builds locais são gerados em `Builds/AJornadaDoCavaleiro` e não são versionados.

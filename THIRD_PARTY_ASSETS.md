# Assets de terceiros

Este projeto usa quatro pacotes gratuitos da Unity Asset Store e cinco arquivos de áudio gratuitos.
As licenças permitem incorporar esse conteúdo ao jogo, mas restringem a redistribuição dos arquivos
originais, isolados ou junto dos arquivos-fonte. Por esse motivo, os binários permanecem disponíveis
apenas no ambiente local do autor e não fazem parte do histórico público deste repositório. Os
scripts, cenas e arquivos `.meta` necessários para manter os GUIDs são versionados.

Referência dos termos: <https://unity.com/legal/as-terms>

## Pacotes e caminhos esperados

### Bandits - Pixel Art

- Product ID: `104130`
- Versão usada: `2.2`
- Página: <https://assetstore.unity.com/packages/2d/characters/bandits-pixel-art-104130>
- Caminho esperado: `Assets/Bandits - Pixel Art/`
- Uso: sprites, animações e prefabs dos bandidos e do líder.

### Hero Knight - Pixel Art

- Product ID: `165188`
- Versão usada: `1.2`
- Página: <https://assetstore.unity.com/packages/2d/characters/hero-knight-pixel-art-165188>
- Caminho esperado: `Assets/Hero Knight - Pixel Art/`
- Uso: sprite, animações, controlador e prefab do cavaleiro.

### Pixel Art Platformer - Village Props

- Product ID: `166114`
- Versão usada: `2.3.2`
- Página: <https://assetstore.unity.com/packages/2d/environments/pixel-art-platformer-village-props-166114>
- Caminhos esperados: `Assets/Cainos/Common/` e
  `Assets/Cainos/Pixel Art Platformer - Village Props/`
- Uso: terreno, árvores, barracas, pontes, provisões, decoração e estandarte final.

### Fantasy Wooden GUI : Free

- Product ID: `103811`
- Versão registrada no projeto: `2.0`
- Versão atual da Asset Store durante a publicação: `2.1`
- Página: <https://assetstore.unity.com/packages/2d/gui/fantasy-wooden-gui-free-103811>
- Caminho esperado: `Assets/Fantasy Wooden GUI  Free/`
- Uso: pergaminhos e botões da interface.

## Áudios e caminhos esperados

Os quatro itens do Pixabay estão sob a
[Pixabay Content License](https://pixabay.com/service/license-summary/), que permite uso e adaptação
em obras maiores, mas proíbe distribuir o conteúdo de forma isolada. O efeito do Mixkit está sob a
[Mixkit Sound Effects Free License](https://mixkit.co/license/modal/sfxFree/), que permite uso em
videogames, mas não redistribuição do item como arquivo-fonte. Atribuição não é obrigatória nessas
licenças, mas as fontes são registradas abaixo para rastreabilidade.

| Caminho local | Fonte | Uso no jogo |
| --- | --- | --- |
| `Assets/EfeitosSonoros/MusicaDeFundoMenu.mp3` | [Medieval Music — kiravale / Pixabay](https://pixabay.com/music/celtic-medieval-music-593694/) | Música do menu |
| `Assets/EfeitosSonoros/MusicaDeFundoDoJogoRodando.mp3` | [Medieval Waltz Music — Tunetank / Pixabay](https://pixabay.com/music/adventure-medieval-waltz-music-412748/) | Música da partida |
| `Assets/EfeitosSonoros/AtaqueEspada.wav` | [Metal Hit Woosh — Mixkit, item 1485](https://mixkit.co/free-sound-effects/sword/) | Ataque de espada |
| `Assets/EfeitosSonoros/escudo.mp3` | [Shield Guard — nekoninja / Pixabay](https://pixabay.com/sound-effects/film-special-effects-shield-guard-6963/) | Defesa com escudo |
| `Assets/EfeitosSonoros/SinoAoMorrer.mp3` | [Bell Toll — Stickypix7996 / Pixabay](https://pixabay.com/sound-effects/film-special-effects-bell-toll-407826/) | Derrota do jogador |

Baixe cada arquivo pela página indicada e renomeie-o exatamente como na primeira coluna. Não apague
nem regenere os `.meta` já versionados: eles preservam as referências serializadas da cena.

## Reprodução

1. Obtenha os quatro pacotes na conta que abrirá o projeto.
2. Importe-os pelo Package Manager (`My Assets`) nos caminhos originais.
3. Baixe os cinco áudios nas páginas indicadas e coloque-os nos caminhos exatos da tabela.
4. Preserve os `.meta` versionados dos áudios e aguarde a Unity concluir importação e compilação.
5. Abra `Assets/Scenes/MainGame.unity`.
6. Execute `Projeto Medieval > Validar jogo completo > MainGame`.

Os GUIDs internos são fornecidos pelos próprios pacotes. Não recrie manualmente seus arquivos
`.meta`. Caso uma versão futura altere nomes ou caminhos, use as versões indicadas acima ou ajuste
somente as constantes de caminho em `Assets/MainGame/Editor/MainGameSceneBuilder.cs` após confirmar
as novas referências.

Esta documentação não substitui aconselhamento jurídico; os termos oficiais da Unity e dos
respectivos publishers prevalecem.

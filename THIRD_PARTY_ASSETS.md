# Assets de terceiros

Este projeto usa quatro pacotes gratuitos da Unity Asset Store. Todos estão identificados nas
metainformações originais como sujeitos à **Standard Unity Asset Store EULA**. A licença permite
usar e distribuir os assets incorporados em um jogo, mas não republicar os arquivos-fonte dos
pacotes como uma coleção reutilizável. Por esse motivo, eles permanecem disponíveis apenas no
ambiente local do autor e não fazem parte do histórico público deste repositório.

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

## Reprodução

1. Obtenha os quatro pacotes na conta que abrirá o projeto.
2. Importe-os pelo Package Manager (`My Assets`) nos caminhos originais.
3. Aguarde a Unity concluir a importação e compilação.
4. Abra `Assets/Scenes/MainGame.unity`.
5. Execute `Projeto Medieval > Validar jogo completo > MainGame`.

Os GUIDs internos são fornecidos pelos próprios pacotes. Não recrie manualmente seus arquivos
`.meta`. Caso uma versão futura altere nomes ou caminhos, use as versões indicadas acima ou ajuste
somente as constantes de caminho em `Assets/MainGame/Editor/MainGameSceneBuilder.cs` após confirmar
as novas referências.

Esta documentação não substitui aconselhamento jurídico; os termos oficiais da Unity e dos
respectivos publishers prevalecem.

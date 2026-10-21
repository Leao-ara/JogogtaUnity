# Protótipo independente de movimentação

Esta pasta não modifica os scripts originais do San Andreas Unity.

## Cena de teste
1. Abra a cena `gta0cena` criada localmente ou crie uma cena 3D vazia.
2. Crie um Plane na posição (0,0,0), escala (10,1,10).
3. Crie uma Capsule na posição (0,1,0).
4. Na Capsule, adicione o componente `Character Controller`. Remova o componente `Capsule Collider` para evitar colisores duplicados.
5. Adicione o componente `MovimentoJogador` na Capsule.
6. Na Main Camera, adicione `CameraSeguidora` e arraste a Capsule para o campo `Alvo`.
7. Pressione Play e use WASD ou as setas.

**Observações:** scripts usam o Input Manager legado, compatível com a configuração atual exibida no Unity. Caso o projeto use exclusivamente o novo Input System, será preciso ajustar Active Input Handling. A cena criada no computador não é enviada ao GitHub automaticamente. Este protótipo não implementa multiplayer nem controles de Android. Não foi testado no Unity Editor.

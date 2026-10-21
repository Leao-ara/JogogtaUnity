# Veículo independente V1 (experimental)

Este componente é um protótipo de direção arcade com Rigidbody, sem dependência de modelos, mapas ou dados do GTA San Andreas.

## Teste isolado, sem modificar gta0cena
1. Faça backup/commit da cena atual.
2. Crie uma cena de teste separada com Plane e um Cube.
3. No Cube, use Scale (1.8, 1, 4), Position (0, 1, 0); adicione Box Collider (normalmente já vem) e Rigidbody.
4. Adicione IndependentArcadeVehicle ao Cube.
5. Marque **Enable Keyboard Control** no Inspector do Cube.
6. Posicione a Main Camera para enxergar o Cube e aperte Play.
7. Use W/S para acelerar e dar ré, A/D para virar, Space para frear.

Este protótipo não implementa rodas, suspensão, entrar/sair, som, câmera veicular, controles touch ou rede. **Não habilite simultaneamente o controle do personagem e do veículo usando o mesmo teclado**, pois ambos reagiriam às teclas. O próximo passo será implementar entrada/saída e alternância de controle/câmera.

Compatibilidade: usa Rigidbody.velocity (Unity 6 ainda suporta essa API com aviso de obsolescência em algumas versões). Não foi testado no Editor; se ocorrerem erros, envie o Console.

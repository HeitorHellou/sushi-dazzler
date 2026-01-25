# SUSHI DAZZLER

Um jogo de ritmo para desktop onde o jogador é um chef de sushi trabalhando em diferentes bares pelo Japão. O jogador prepara pratos de sushi sincronizando suas ações com o ritmo da música.

## Visão Geral do Projeto

- **Plataforma**: PC (Desktop)
- **Framework**: MonoGame (.NET 9.0)
- **Prazo**: 6 meses
- **Equipe**: 2 programadores (+ artista futuro)

## Como Rodar

```bash
# Compilar
dotnet build sushi-dazzler/sushi-dazzler.csproj

# Executar
dotnet run --project sushi-dazzler/sushi-dazzler.csproj
```

## Controles

| Tecla   | Ação                    |
|---------|-------------------------|
| Enter   | Iniciar música          |
| Space   | TAP (cortar sushi)      |
| H       | HOLD (moldar sushi)     |
| Esc     | Sair                    |

## Estrutura do Projeto

```
sushi-dazzler/
├── Core/                    # Sistemas principais
│   ├── Conductor.cs         # Controle de tempo/ritmo
│   ├── Song.cs              # Estrutura de dados da música
│   ├── Note.cs              # Estrutura de dados das notas
│   ├── SongLoader.cs        # Carregamento de JSON
│   ├── NoteTracker.cs       # Detecção de acertos
│   └── NoteHighway.cs       # Visualização das notas
├── Content/
│   ├── Songs/               # Charts das músicas (JSON)
│   │   └── yokohama/
│   │       └── easy.json
│   └── DefaultFont.spritefont
└── Game1.cs                 # Loop principal do jogo
```

## Sistemas Principais

### 1. Conductor (Maestro)

O Conductor é a **fonte única de verdade** para o tempo musical. Todos os outros sistemas consultam o Conductor para saber "em que momento da música estamos".

**Propriedades importantes:**
- `SongPosition`: Tempo decorrido em segundos
- `BPM`: Batidas por minuto (velocidade da música)
- `CurrentBeat`: Em qual batida estamos (número decimal)
- `Crotchet`: Duração de uma batida em segundos (60 / BPM)

**Localização**: `Core/Conductor.cs`

### 2. Song & Note (Música e Notas)

Estruturas de dados que representam uma música e suas notas. Carregadas de arquivos JSON.

**Localização**: `Core/Song.cs`, `Core/Note.cs`, `Core/SongLoader.cs`

### 3. NoteTracker (Rastreador de Notas)

Faz a ponte entre o Conductor e a Song. Determina quais notas estão "ativas" (podem ser acertadas) e registra acertos/erros.

- `HitWindow`: Janela de tolerância (±0.2 batidas por padrão)
- `ActiveNotes`: Notas dentro da janela de acerto
- `HitCount / MissCount`: Estatísticas

**Localização**: `Core/NoteTracker.cs`

### 4. NoteHighway (Pista de Notas)

Sistema visual que mostra as notas se aproximando da zona de acerto. As notas vêm da direita e se movem para a esquerda.

**Localização**: `Core/NoteHighway.cs`

## Entendendo Beats e BPM

### O que é um Beat?

**Beat (batida)** é a unidade fundamental de tempo musical. No nosso sistema, usamos beats para definir quando as notas devem aparecer, ao invés de usar segundos ou milissegundos.

### Por que usar Beats?

1. **Intuição Musical**: É mais natural pensar "essa nota cai no beat 4" do que "essa nota cai em 2000ms"
2. **Independência do BPM**: Se mudar o BPM, todas as notas se ajustam automaticamente
3. **Facilidade para Charting**: Alinhar notas aos beats 1, 2, 3, 4 é mais intuitivo

### A Matemática

```
BPM = 120 (batidas por minuto)

Crotchet = 60 / BPM = 60 / 120 = 0.5 segundos por beat

CurrentBeat = SongPosition / Crotchet
            = 1.5 segundos / 0.5 = beat 3.0

Tempo da Nota = Note.Beat × Crotchet
              = 4.0 × 0.5 = 2.0 segundos
```

**Exemplo prático (120 BPM):**
- Uma nota no beat 4.0 deve ser acertada aos 2.0 segundos
- Uma nota no beat 8.0 deve ser acertada aos 4.0 segundos

### Subdivisões de Beat

Notas podem cair em frações de beat:

```
Beat:    1.0    1.25   1.5    1.75   2.0
         │      │      │      │      │
         ▼      ▼      ▼      ▼      ▼
         1    1+1/4  1+1/2  1+3/4    2

1.0   = Na batida (semínima)
1.5   = Meia batida (colcheia)
1.25  = Quarto de batida (semicolcheia)
```

## Formato dos Charts (JSON)

Os charts ficam em `Content/Songs/{bar}/{dificuldade}.json`

```json
{
  "title": "Yokohama Nights",
  "artist": "dosii",
  "bpm": 120,
  "audioFile": "yokohama.ogg",
  "offset": 0.0,
  "notes": [
    { "beat": 1.0, "type": "Tap" },
    { "beat": 2.0, "type": "Tap" },
    { "beat": 4.0, "type": "Tap" },
    { "beat": 8.0, "type": "Hold", "duration": 2.0 }
  ]
}
```

| Campo      | Descrição                                          |
|------------|----------------------------------------------------|
| `title`    | Nome da música                                     |
| `artist`   | Nome do artista                                    |
| `bpm`      | Batidas por minuto                                 |
| `audioFile`| Arquivo de áudio (relativo à pasta da música)      |
| `offset`   | Segundos antes do beat 0 (para sincronizar áudio)  |
| `notes`    | Array de notas                                     |

**Campos das notas:**
| Campo      | Descrição                                          |
|------------|----------------------------------------------------|
| `beat`     | Quando a nota deve ser acertada (em beats)         |
| `type`     | `"Tap"` ou `"Hold"`                                |
| `duration` | Apenas para Hold: duração em beats                 |

## Fluxo do Sistema

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ Arquivo JSON│────▶│    Song     │────▶│ NoteTracker │
│   (chart)   │     │   (notas)   │     │  (ativas)   │
└─────────────┘     └─────────────┘     └─────────────┘
                                               │
                                               ▼
┌─────────────┐                        ┌─────────────┐
│  GameTime   │───────────────────────▶│  Conductor  │
│ (segundos)  │                        │(CurrentBeat)│
└─────────────┘                        └─────────────┘
                                               │
      ┌────────────────────────────────────────┘
      ▼
"CurrentBeat está dentro de ±0.2 do Note.Beat?"
      │
      ├─── SIM ──▶ Nota está ATIVA (pode ser acertada)
      │
      └─── NÃO ──▶ Nota ainda não ativa OU foi perdida
```

## Gameplay Planejado

### Mecânicas
- **TAP**: Pressionar no momento certo (cortar sushi)
- **HOLD**: Segurar por uma duração (moldar sushi)

### Estrutura do Jogo
- Bares de sushi espalhados pelo Japão
- Cada bar tem seu tema e estilo musical
- Sem "game over" - erros apenas diminuem pontuação final
- Feedback de 1-5 estrelas
- 3 dificuldades por bar (fácil/médio/difícil)

### Bares Planejados
- **Yokohama**: City pop (referência: Goyeol - dosii)
- **Osaka**: J-Rock (referência: rock 'n' roll wa shinanai - haru nemuri)

### Arte
- 2D Pixel Art
- Câmera focada no chef para ver as animações

## Stack Técnica

- **Framework**: MonoGame.Framework.DesktopGL v3.8.*
- **Linguagem**: C# / .NET 9.0
- **IDE**: Visual Studio Code
- **Content Pipeline**: MGCB (MonoGame Content Builder)

## Próximos Passos

- [ ] Integrar áudio real com o Conductor
- [ ] Implementar sistema de pontuação
- [ ] Criar mais charts
- [ ] Adicionar animações do chef
- [ ] Menu principal e seleção de músicas

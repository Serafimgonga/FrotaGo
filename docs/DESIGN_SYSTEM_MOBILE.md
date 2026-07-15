# Design System — FrotaGo Mobile

## Paleta de Cores

A app mobile segue um design simples e profissional, alinhado com Material Design 3 e inspirado no Angular Material do webapp.

### Cores Principais

| Elemento | Cor | Hex | Uso |
|----------|-----|-----|-----|
| **Primária** | Azul | `#2196F3` | Botões principais, headers, links ativos |
| **Primária Escura** | Azul Escuro | `#1976D2` | Hover/Pressed states |
| **Sucesso** | Verde | `#4CAF50` | Status OK, confirmações |
| **Erro** | Vermelho | `#F44336` | Erros, validações negativas |
| **Aviso** | Laranja | `#FF9800` | Alertas, atenção |
| **Fundo Principal** | Branco | `#FFFFFF` | Fundo das páginas |
| **Fundo Secundário** | Cinzento Claro | `#F5F5F5` | Cards, secções |
| **Texto Primário** | Preto | `#212121` | Texto principal |
| **Texto Secundário** | Cinzento Escuro | `#757575` | Subtítulos, labels |
| **Borders** | Cinzento Médio | `#E0E0E0` | Separadores, borders |

## Tipografia

- **Headers:** Bold, 20pt (títulos de página)
- **Subtítulos:** SemiBold, 16pt (títulos de seção)
- **Corpo:** Regular, 14pt (texto de corpo)
- **Labels:** Regular, 12pt (labels de input, captions)

## Componentes

### Botões

#### Primário
- Background: `#2196F3`
- Text: Branco
- Padding: 12pt horizontal, 10pt vertical
- Border Radius: 4pt

#### Secundário
- Background: `#F5F5F5`
- Text: `#212121`
- Border: 1pt `#E0E0E0`
- Padding: 12pt horizontal, 10pt vertical

### Inputs/Fields

- Background: Branco
- Border: 1pt `#E0E0E0`
- Focus Border: 2pt `#2196F3`
- Placeholder Text: `#999999`
- Padding: 12pt

### Cards

- Background: Branco
- Shadow: Subtil (Android: elevation 2, iOS: UIColor.separator)
- Border Radius: 8pt
- Padding: 16pt

### Headers/AppBar

- Background: `#2196F3`
- Text: Branco
- Height: 56pt

## Layouts

### Safe Area
- Padding: 16pt (top, left, right, bottom)
- Aplicado a todas as páginas

### Espaçamento Vertical
- Entre seções: 24pt
- Entre elementos: 16pt
- Entre labels e inputs: 8pt

## Ícones

Utilizar ícones de Material Design (fornecidos pelo .NET MAUI Community Toolkit)

Exemplos:
- Login: `lock` icon
- Aulas: `list` icon
- Início: `play` icon
- Parada: `stop` icon
- GPS: `location_on` icon

## Estados Visuais

### Loading
- Spinner circular (activity indicator)
- Cor: `#2196F3`

### Disabled
- Opacity: 50%
- Cursor: not-allowed

### Hover (Desktop/Web)
- Fundo: Cinzento claro `#F5F5F5`
- Transição: 200ms

## Dark Mode (Futuro)

Se implementado:
- Fundo Principal: `#121212`
- Fundo Secundário: `#1E1E1E`
- Texto Primário: Branco
- Primária: `#64B5F6` (mais clara)

## Referências

- [Material Design 3](https://m3.material.io/)
- [Angular Material Theming](https://material.angular.io/guide/theming)
- [MAUI Styles](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/styles/xaml/)

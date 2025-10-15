# СТБ 34.101.45 ЭЦП на эллиптических кривых

## Сборка

Полсе `git clone` в корне репозитория выполнить:

```bash
dotnet restore
dotnet build --no-restore
cp Core/TZICrypt.dll Core.Tests/bin/Debug/net9.0/
cp Core/gp.exe Core.Tests/bin/Debug/net9.0/
```
> Для запуска тестов `dotnet test --no-build --verbosity normal`

Копирование `TZICrypt.dll` и `gp.exe` необходимо для успешного прохождения тестов.

## Функциональальность библиотеки

### Электоронная цифровая подпись (ЭЦП)
[EDS.cs](./Core/EDS.cs)

#### Генерация подписи
```cs
public static byte[] Generate(EllipticCurve curve, byte[] X, BigInteger d, (byte[] OID, byte[] H, BigInteger k)? data = null)
```
`EllipticCurve curve` - эллиптическая кривая над которой выполняются операции.
`byte[] X` - Сообщение, которое необходимо подписать.
`BigInteger d` - Закрытый ключ.
`(byte[] OID, byte[] H, BigInteger k)? data = null` - Дополнительные параметры:
- `byte[] OID` - Идентификатор алгоритма хеширования.
- `byte[] H` - Хеш сообщения.
- `BigInteger k` - Одноразовый ключ.

Возвращает подпись.

#### Проверка подписи
```cs
 public static bool Check(EllipticCurve curve, byte[] X, byte[] S, ECPoint Q, (byte[] OID, byte[] H)? data = null)
```
- `EllipticCurve curve` - эллиптическая кривая над которой выполняются операции.
- `byte[] X` - Сообщение, подпись которого необходимо проверить.
- `byte[] S` - Подпись.
- `ECPoint Q` - Открытый ключ.
- `(byte[] OID, byte[] H)? data = null` - Дополнительные параметры:
	 - `byte[] OID` - Идентификатор алгоритма хеширования.
	 - `byte[] H` - Хеш сообщения.

Возвращает `true`, если подпись верна, иначе `false`.

### Генератор ключей
[KeyGenerator.cs](./Core/KeyGenerator.cs)

#### Генерация ключевой пары
```cs
public static (BigInteger d, ECPoint Q) GenerateKey(EllipticCurve curve, BigInteger? d = null)
```
- `EllipticCurve curve` - эллиптическая кривая над которой выполняются операции.
- `BigInteger? d = null` - Закрытый ключ. Если не задан, то генерируется случайным образом.

Возвращает кортеж из закрытого ключа `d` и открытого ключа `Q`.

#### Валидация открытого ключа
```cs
public static bool CheckKey(ECPoint Q, EllipticCurve curve)
```
- `ECPoint Q` - Открытый ключ.
- `EllipticCurve curve` - эллиптическая кривая над которой выполняются операции.

Возвращает `true`, если открытый ключ валиден, иначе `false`.

#### Генерация одноразового ключа
```cs
public static BigInteger GenerateOneTimeKey(BigInteger q, BigInteger d, byte[] H, byte[]? t = null, byte[]? theta = null)
```
- `BigInteger q` - Порядок группы точек эллиптической кривой.
- `BigInteger d` - Закрытый ключ.
- `byte[] H` - Хеш сообщения.
- `byte[]? t = null` - Дополнительные данные (пустое слово по умолчанию).
- `byte[]? theta = null` - Дополнительные данные.

Возвращает одноразовый ключ.


## Эллиптическая кривая  
[EllipticCurve.cs](./Core/EllipticCurve.cs)

### Назначение
Класс **`EllipticCurve`** реализует математическую модель эллиптической кривой над конечным полем $F_p$ вида $y^2 = x^3 + ax + b \pmod p$
и операции, необходимые для построения и проверки параметров в соответствии со стандартом **СТБ 34.101.45**.

### Стандартные параметры
В классе определены константы для стандартной кривой **СТБ 34.101.45, уровень безопасности l = 128 бит**:
```cs
public const string P_DEFAULT_STB_128_BASE_16
public const string A_DEFAULT_STB_128_BASE_16
public const string B_DEFAULT_STB_128_BASE_16
public const string SEED_DEFAULT_STB_128_BASE_16
public const string Q_DEFAULT_STB_128_BASE_16
public const string GY_DEFAULT_STB_128_BASE_16
```

Для получения стандартной кривой используется метод:
```cs
public static EllipticCurve GetStandardCurve()
```
Возвращает готовую реализацию кривой **СТБ 34.101.45, l=128**, с параметрами (p, a, b, q, G).

### Основные свойства

| Свойство  | Тип          | Описание                                                   |
|-----------|--------------|------------------------------------------------------------|
| `P`       | `BigInteger` | Модуль конечного поля (простое число).                     |
| `A`       | `BigInteger` | Коэффициент `a` в уравнении кривой.                        |
| `B`       | `BigInteger` | Коэффициент `b` в уравнении кривой.                        |
| `Seed`    | `BigInteger` | Случайное значение, используемое при генерации параметров. |
| `Q`       | `BigInteger` | Порядок базовой точки.                                     |
| `G`       | `ECPoint`    | Базовая точка кривой.                                      |

### Основные методы

#### Проверка принадлежности точки кривой
```cs
public bool IsOnCurve(ECPoint point)
```
Возвращает `true`, если точка `point` удовлетворяет уравнению кривой, иначе `false`.

#### Вычисление базовой точки
```cs
public static ECPoint ComputeBasePoint(BigInteger p, BigInteger b)
```
Вычисляет базовую точку $G = (0, b^{(p+1)/4} \mod p)$ для кривой с заданными параметрами `p` и `b`.

#### Генерация параметров кривой
```cs
public static EllipticCurve GenerateCurveParameters(BigInteger p, BigInteger a, int l = 128)
```
- `p` - Модуль конечного поля (простое число).
- `a` - Коэффициент `a` в уравнении кривой.
- `l` - Уровень безопасности (битовая длина порядка точки `q`), допустимые значения: 128, 192, 256 (по умолчанию 128).

Генерирует корректные параметры эллиптической кривой:
- случайное значение `seed`;
- коэффициент `b` с использованием криптографического хеша `belt-hash`;
- порядок точки `q`;
- базовую точку `G`.

#### Проверка корректности параметров
```cs
public bool CheckParams()
```
Проверяет корректность параметров кривой по алгоритму СТБ 34.101.45

Возвращает `true`, если все условия выполняются, иначе `false`.

#### Определение параметра l

```cs
public int GetL()
```
Вычисляет минимальное значение `l`, для которого $p < 2^{2l}$.

## Точка эллиптической кривой
[ECPoint.cs](./Core/ECPoint.cs)

### Назначение
Класс **`ECPoint`** реализует математическую модель точки эллиптической кривой и операции над точками, необходимые для построения и проверки параметров в соответствии со стандартом **СТБ 34.101.45**.

### Основные свойства
| Свойство     | Тип          | Описание                          |
|--------------|--------------|-----------------------------------|
| `X`          | `BigInteger` | Координата `x` точки.             |
| `Y`          | `BigInteger` | Координата `y` точки.             |
| `IsInfinity` | `bool`       | Флаг бесконечной точки (точки O). |

### Основные методы

#### Нахождение кратной точки
```cs
public static ECPoint MultiplyScalar(ECPoint P, BigInteger d, EllipticCurve curve)

public static ECPoint MultiplyScalarNAF(ECPoint P, BigInteger d, EllipticCurve curve)

public static ECPoint MultiplyScalarAdditiveChain(ECPoint P, BigInteger d, EllipticCurve curve, List<(int, int)>? chain = null)

public static ECPoint MultiplyScalarWindow(ECPoint P, BigInteger d, EllipticCurve curve, int w = 4)

public static ECPoint MultiplyScalarSlidingWindow(ECPoint P, BigInteger d, EllipticCurve curve, int w = 4)

public static ECPoint MultiplyScalarJacobian(ECPoint P, BigInteger d, EllipticCurve curve)
```
- `ECPoint P` - Точка.
- `BigInteger d` - скаляр.
- `EllipticCurve curve` - эллиптическая кривая над которой выполняются операции.

Возращает точку d*P, использует бинарный метод справа налево.

| Method                                   | Mean        | Error       | StdDev      | Ratio | RatioSD | Rank | Gen0      | Allocated   | Alloc Ratio |
|----------------------------------------- |------------:|------------:|------------:|------:|--------:|-----:|----------:|------------:|------------:|
| 'Binary multiply'                        | 25,275.2 us |   480.24 us |   553.04 us |  1.00 |    0.03 |    4 |         - |   274.12 KB |        1.00 |
| 'NAF multiply'                           | 23,376.5 us |   314.38 us |   294.07 us |  0.93 |    0.02 |    3 |         - |   254.68 KB |        0.93 |
| 'Additive chain multiply prebuild chain' | 25,363.9 us |   472.11 us |   463.68 us |  1.00 |    0.03 |    4 |         - |   273.54 KB |        1.00 |
| 'Additive chain multiply'                | 72,288.6 us | 1,418.34 us | 1,576.48 us |  2.86 |    0.09 |    5 | 7285.7143 | 90494.48 KB |      330.13 |
| 'Window method (w=4)'                    | 22,720.1 us |   446.10 us |   495.84 us |  0.90 |    0.03 |    3 |         - |   242.16 KB |        0.88 |
| 'Sliding window (w=4)'                   | 20,754.5 us |   407.02 us |   435.51 us |  0.82 |    0.02 |    2 |         - |    228.1 KB |        0.83 |
| 'Jacobian coordinates'                   |    871.6 us |    17.42 us |    20.06 us |  0.03 |    0.00 |    1 |   43.9453 |   550.19 KB |        2.01 |

#### Сложение двух точек
```cs
public static ECPoint Add(ECPoint p, ECPoint q, EllipticCurve curve)
```

#### Удвоение точки
```cs
public static ECPoint Double(ECPoint p, EllipticCurve curve) => Add(p, p, curve);
```

#### Трюк Шамира
```cs
public static ECPoint ShamirTrick(BigInteger d, ECPoint P, BigInteger e, ECPoint Q, EllipticCurve curve)
```
Находит $dP + eQ$ над кривой `curve` используя трюк Шамира.

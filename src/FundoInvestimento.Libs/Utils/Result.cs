using System.Diagnostics.CodeAnalysis;

namespace FundoInvestimento.Libs.Utils;

/// <summary>
/// Representa o resultado de uma operação que não retorna valor, indicando sucesso ou falha.
/// </summary>
[ExcludeFromCodeCoverage]
public class Result
{
    /// <summary>
    /// Indica se a operação foi concluída com sucesso.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indica se a operação falhou.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    private readonly CustomError? _error;

    /// <summary>
    /// Construtor protegido para inicializar o estado do resultado.
    /// </summary>
    /// <param name="isSuccess">Flag de sucesso.</param>
    /// <param name="error">Objeto de erro (nulo se sucesso).</param>
    /// <exception cref="InvalidOperationException">Lançada se os parâmetros forem inconsistentes.</exception>
    protected Result(bool isSuccess, CustomError? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("Um resultado de sucesso não pode conter um erro.");

        if (!isSuccess && error is null)
            throw new InvalidOperationException("Um resultado de falha deve conter um erro.");

        IsSuccess = isSuccess;
        _error = error;
    }

    /// <summary>
    /// Obtém o erro associado à falha.
    /// </summary>
    /// <returns>O objeto <see cref="CustomError"/> detalhado.</returns>
    /// <exception cref="InvalidOperationException">Lançada se a operação foi um sucesso.</exception>
    public CustomError GetError() => _error ?? throw new InvalidOperationException("Não é possível obter erro de um resultado de sucesso.");

    /// <summary>
    /// Cria um resultado de sucesso.
    /// </summary>
    /// <returns>Uma nova instância de <see cref="Result"/> indicando sucesso.</returns>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Cria um resultado de falha.
    /// </summary>
    /// <param name="error">O objeto de erro detalhado.</param>
    /// <returns>Uma nova instância de <see cref="Result"/> indicando falha.</returns>
    public static Result Failure(CustomError error) => new(false, error);
}

/// <summary>
/// Representa o resultado de uma operação que retorna um valor do tipo <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">O tipo do valor retornado em caso de sucesso.</typeparam>
[ExcludeFromCodeCoverage]
public class Result<T> : Result
{
    private readonly T? _value;

    private Result(bool isSuccess, CustomError? error, T? value)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Obtém o valor gerado pela operação em caso de sucesso.
    /// </summary>
    /// <returns>O valor do tipo <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">Lançada se a operação foi uma falha.</exception>
    public T GetSuccess() => IsSuccess ? _value! : throw new InvalidOperationException("Não é possível acessar o valor de um resultado com falha.");

    /// <summary>
    /// Cria um resultado de sucesso contendo um valor.
    /// </summary>
    /// <param name="value">O valor a ser retornado.</param>
    /// <returns>Uma nova instância de <see cref="Result{T}"/> indicando sucesso.</returns>
    public static Result<T> Success(T value) => new(true, null, value);

    /// <summary>
    /// Cria um resultado de falha.
    /// </summary>
    /// <param name="error">O objeto de erro detalhado.</param>
    /// <returns>Uma nova instância de <see cref="Result{T}"/> indicando falha.</returns>
    public static new Result<T> Failure(CustomError error) => new(false, error, default);
}
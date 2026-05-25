namespace ZMap;

public record LayerQueryResult(List<Layer> Layers, int FetchCount, List<int> LayerQueryIndices);
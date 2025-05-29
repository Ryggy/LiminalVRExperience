
public static class EmitterStrategyFactory
{
    public static IEmitterStrategy GetStrategy(FlowField2DParticleEmitter.Emission type)
    {
        switch (type)
        {
            case FlowField2DParticleEmitter.Emission.VectorOrigins:
                return new VectorOriginEmitter();
            case FlowField2DParticleEmitter.Emission.InsideField:
                return new InsideFieldEmitter();
            case FlowField2DParticleEmitter.Emission.Corners:
                return new CornersEmitter();
            case FlowField2DParticleEmitter.Emission.Edges:
                return new EdgesEmitter();
            case FlowField2DParticleEmitter.Emission.Surface:
                return new SurfaceEmitter();
            default:
                throw new System.ArgumentOutOfRangeException();
        }
    }
}

